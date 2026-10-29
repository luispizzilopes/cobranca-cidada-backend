using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cobranca.Cidada.Application.Modules.Commitment.Commands.SeedCommitments;
using Cobranca.Cidada.Application.Modules.Commitment.Requests;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Infrastructure.Context;
using Cobranca.Cidada.Tests.Application.Commitment;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cobranca.Cidada.Tests.Integration;

[Collection(PostgresCollection.Name)]
public class CommitmentsIntegrationTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly PostgresFixture _postgres;

    public CommitmentsIntegrationTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    private static SeedCommitmentsCommand SeedCommand(SeedFileRequest file, bool overwrite = false)
        => new(file, overwrite, new DateOnly(2027, 1, 5), new DateOnly(2030, 12, 31));

    private static async Task<SeedReportResponse> SeedAsync(ApiFactory factory, SeedFileRequest file, bool overwrite = false)
    {
        using var scope = factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IMediator>().Send(SeedCommand(file, overwrite));
        Assert.False(result.IsError, result.IsError ? result.FirstError.Description : null);
        return result.Value;
    }

    [Fact]
    public async Task Seed_CreatesAllCommitmentsAreasAndOrigins()
    {
        await using var factory = await _postgres.CreateFactoryAsync();
        var file = SeedFileLocator.Load();

        var report = await SeedAsync(factory, file);

        Assert.Equal(file.Commitments.Count, report.Created);
        Assert.Empty(report.Rejected);
        Assert.Equal(20, report.AreasCreated);
        Assert.Equal(4, report.OriginsCreated);
    }

    [Fact]
    public async Task Seed_RerunIsIdempotent_AndSkipsExistingIds()
    {
        await using var factory = await _postgres.CreateFactoryAsync();
        var file = SeedFileLocator.Load();
        await SeedAsync(factory, file);

        var second = await SeedAsync(factory, file);

        Assert.Equal(0, second.Created);
        Assert.Equal(file.Commitments.Count, second.Skipped);
        using var scope = factory.Services.CreateScope();
        Assert.Equal(file.Commitments.Count, await scope.ServiceProvider.GetRequiredService<AppDbContext>().Commitments.CountAsync());
    }

    [Fact]
    public async Task Seed_Rerun_DoesNotOverwriteEditedCommitments_UnlessOverwriteFlagIsUsed()
    {
        await using var factory = await _postgres.CreateFactoryAsync();
        var file = SeedFileLocator.Load();
        await SeedAsync(factory, file);

        using (var scope = factory.Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var commitment = (await uow.CommitmentRepository.GetByExternalIdAsync("LULA26-ECON-006", default))!;
            var edited = commitment.ToContent() with { Title = "Título editado pela curadoria" };
            commitment.Update(edited, commitment.AreaId, commitment.OriginId, "curador-1", "Ajuste editorial");
            await uow.CommitAsync();
        }

        await SeedAsync(factory, file);
        Assert.Equal("Título editado pela curadoria", await TitleOfAsync(factory, "ECON-006"));

        var overwrite = await SeedAsync(factory, file, overwrite: true);
        Assert.Equal(file.Commitments.Count, overwrite.Overwritten);
        Assert.NotEqual("Título editado pela curadoria", await TitleOfAsync(factory, "ECON-006"));

        using var check = factory.Services.CreateScope();
        var revisions = await check.ServiceProvider.GetRequiredService<AppDbContext>().CommitmentRevisions.CountAsync();
        Assert.Equal(file.Commitments.Count + 1, revisions);
    }

    [Fact]
    public async Task Seed_RejectsDuplicateIdsInvalidStatusAndBadUrls_WithReasons()
    {
        await using var factory = await _postgres.CreateFactoryAsync();
        var good = SeedFileLocator.Load().Commitments.First();
        var file = new SeedFileRequest
        {
            Metadata = new SeedMetadataRequest { Candidate = "Fulano de Tal (XYZ)" },
            Commitments =
            [
                good,
                good,
                Clone(good, c => { c.Id = "LULA26-BAD-001"; c.Execution!.Status = "Quase cumprido"; }),
                Clone(good, c => { c.Id = "LULA26-BAD-002"; c.Sources![0].Url = "isto-nao-e-url"; }),
            ]
        };

        var report = await SeedAsync(factory, file);

        Assert.Equal(1, report.Created);
        Assert.Equal(3, report.Rejected.Count);
        Assert.Contains(report.Rejected, r => r.Reasons.Any(x => x.Contains("duplicado")));
        Assert.Contains(report.Rejected, r => r.Id == "LULA26-BAD-001");
        Assert.Contains(report.Rejected, r => r.Id == "LULA26-BAD-002");
    }

    [Fact]
    public async Task Api_ListsWithServerSidePaginationFiltersAndSearch()
    {
        await using var factory = await _postgres.CreateFactoryAsync();
        var file = SeedFileLocator.Load();
        await SeedAsync(factory, file);
        using var client = factory.CreateClient();

        var page = await GetAsync<PaginedResult<CommitmentSummaryResponse>>(client, "/api/v1/commitments?pageNumber=2&pageSize=10");
        Assert.Equal(file.Commitments.Count, page.TotalCount);
        Assert.Equal(10, page.Items.Count());
        Assert.Equal(2, page.PageNumber);

        var econ = await GetAsync<PaginedResult<CommitmentSummaryResponse>>(client, "/api/v1/commitments?area=ECON&pageSize=100");
        Assert.All(econ.Items, i => Assert.Equal("ECON", i.Area.Code));
        Assert.Equal(file.Commitments.Count(c => c.AreaCode == "ECON"), econ.TotalCount);

        var running = await GetAsync<PaginedResult<CommitmentSummaryResponse>>(client, "/api/v1/commitments?status=Em%20andamento&pageSize=100");
        Assert.Equal(file.Commitments.Count(c => c.Execution!.Status == "Em andamento"), running.TotalCount);

        var withDeadline = await GetAsync<PaginedResult<CommitmentSummaryResponse>>(client, "/api/v1/commitments?hasDeadline=true&pageSize=100");
        Assert.Equal(file.Commitments.Count(c => c.DeadlineText is not null), withDeadline.TotalCount);

        var search = await GetAsync<PaginedResult<CommitmentSummaryResponse>>(client, "/api/v1/commitments?search=arcabouço&pageSize=100");
        Assert.NotEmpty(search.Items);
        Assert.Contains(search.Items, i => i.Id == "ECON-001");
    }

    [Fact]
    public async Task Api_Detail_ReturnsSourcesStatusHistoryAndPlanFields_And404WhenMissing()
    {
        await using var factory = await _postgres.CreateFactoryAsync();
        await SeedAsync(factory, SeedFileLocator.Load());
        using var client = factory.CreateClient();

        var detail = await GetAsync<CommitmentDetailResponse>(client, "/api/v1/commitments/ECON-006");
        Assert.Equal("LULA26-ECON-006", detail.ExternalId);
        Assert.Equal("InProgress", detail.Status.Value);
        Assert.NotEmpty(detail.Sources);
        Assert.Single(detail.StatusHistory);

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/v1/commitments/NAO-EXISTE")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/v1/commitments?pageSize=500")).StatusCode);
    }

    [Fact]
    public async Task Api_HidesUnpublishedCommitments()
    {
        await using var factory = await _postgres.CreateFactoryAsync();
        await SeedAsync(factory, SeedFileLocator.Load());

        using (var scope = factory.Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var commitment = (await uow.CommitmentRepository.GetByExternalIdAsync("LULA26-ECON-006", default))!;
            commitment.Unpublish();
            await uow.CommitAsync();
        }

        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/v1/commitments/ECON-006")).StatusCode);
        var page = await GetAsync<PaginedResult<CommitmentSummaryResponse>>(client, "/api/v1/commitments?pageSize=100&area=ECON");
        Assert.DoesNotContain(page.Items, i => i.Id == "ECON-006");
    }

    [Fact]
    public async Task Application_DoesNotDependOnTheJsonFileAtRuntime()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"promessas-{Guid.NewGuid():N}.json");
        File.Copy(SeedFileLocator.Path(), temp);

        await using var factory = await _postgres.CreateFactoryAsync();
        await SeedAsync(factory, JsonSerializer.Deserialize<SeedFileRequest>(File.ReadAllText(temp))!);

        File.Delete(temp);
        Assert.False(File.Exists(temp));

        using var client = factory.CreateClient();
        foreach (var url in new[] { "/api/v1/commitments", "/api/v1/commitments/ECON-001", "/api/v1/catalog", "/api/v1/commitments?search=saúde" })
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(url)).StatusCode);
    }

    private static async Task<string> TitleOfAsync(ApiFactory factory, string slug)
    {
        using var client = factory.CreateClient();
        return (await GetAsync<CommitmentDetailResponse>(client, $"/api/v1/commitments/{slug}")).Title;
    }

    private static async Task<T> GetAsync<T>(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }

    private static SeedCommitmentRequest Clone(SeedCommitmentRequest source, Action<SeedCommitmentRequest> change)
    {
        var copy = JsonSerializer.Deserialize<SeedCommitmentRequest>(JsonSerializer.Serialize(source))!;
        change(copy);
        return copy;
    }
}
