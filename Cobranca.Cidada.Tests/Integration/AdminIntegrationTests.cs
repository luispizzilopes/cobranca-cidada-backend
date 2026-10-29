using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.Commands.SeedCommitments;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using Cobranca.Cidada.Domain.Modules.Audit;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Cobranca.Cidada.Infrastructure.Context;
using Cobranca.Cidada.Tests.Application.Commitment;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cobranca.Cidada.Tests.Integration;

[Collection(PostgresCollection.Name)]
public class AdminIntegrationTests
{
    private const string StrongPassword = "Senh@-Muito-Forte-123";
    private const string AdminBase = "/api/v1/admin/commitments";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly PostgresFixture _postgres;

    public AdminIntegrationTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    private async Task<ApiFactory> NewSeededFactoryAsync()
    {
        var factory = await _postgres.CreateFactoryAsync();
        using var scope = factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IMediator>().Send(
            new SeedCommitmentsCommand(SeedFileLocator.Load(), false, new DateOnly(2027, 1, 5), new DateOnly(2030, 12, 31)));
        Assert.False(result.IsError);
        return factory;
    }

    private static async Task RegisterAsync(ApiFactory factory, HttpClient client, string email)
    {
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/authentication/request-registration-code", new { email })).StatusCode);
        var code = factory.Emails.LastCodeFor(email);
        (await client.PostAsJsonAsync("/api/v1/authentication/register",
            new { name = "Pessoa Teste", email, password = StrongPassword, code, acceptedPrivacyPolicy = true })).EnsureSuccessStatusCode();
    }

    private static async Task<LoginResponse> LoginAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/v1/authentication/login", new { email, password = StrongPassword });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponse>(Json))!;
    }

    private static async Task SetAdminFlagAsync(ApiFactory factory, string email, bool isAdmin)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await context.Users.SingleAsync(u => u.Email == email);
        user.IsAdmin = isAdmin;
        await context.SaveChangesAsync();
    }

    /// <summary>Cria uma conta, marca a flag no banco e entra de novo (a claim só nasce no login).</summary>
    private static async Task<string> AdminTokenAsync(ApiFactory factory, HttpClient client, string email = "admin@example.com")
    {
        await RegisterAsync(factory, client, email);
        await SetAdminFlagAsync(factory, email, true);
        return (await LoginAsync(client, email)).Token;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string? token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) request.Content = JsonContent.Create(body);
        return request;
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }

    private static object ToUpdate(AdminCommitmentDetailResponse c, string reason = "Revisão editorial", Action<Dictionary<string, object?>>? change = null)
    {
        var body = new Dictionary<string, object?>
        {
            ["expectedVersion"] = c.Version,
            ["reason"] = reason,
            ["areaCode"] = c.Area.Code,
            ["originCode"] = c.Origin.Code,
            ["title"] = c.Title,
            ["description"] = c.Description,
            ["shortDescription"] = c.ShortDescription,
            ["howItWillBeDone"] = c.HowItWillBeDone,
            ["hasPlanDetail"] = c.HasPlanDetail,
            ["date"] = c.Date,
            ["dateKind"] = c.DateKind,
            ["deadlineText"] = c.DeadlineText,
            ["deadlineDate"] = c.DeadlineDate,
            ["goalOrCondition"] = c.GoalOrCondition,
            ["classification"] = c.Classification,
            ["evidence"] = c.Evidence,
            ["planPage"] = c.PlanPage,
            ["sources"] = c.Sources.Select(s => new { s.Name, s.Url, s.Page, s.Kind }).ToList()
        };
        change?.Invoke(body);
        return body;
    }

    private static Task<HttpResponseMessage> GetAdminAsync(HttpClient client, string token, string id)
        => client.SendAsync(Authorized(HttpMethod.Get, $"{AdminBase}/{id}", token));

    [Fact]
    public async Task AdminRoutes_RequireTheAdminFlag()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(AdminBase)).StatusCode);

        await RegisterAsync(factory, client, "cidadao@example.com");
        var citizen = await LoginAsync(client, "cidadao@example.com");
        Assert.DoesNotContain("Admin", citizen.Roles);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(Authorized(HttpMethod.Get, AdminBase, citizen.Token))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(Authorized(HttpMethod.Put, $"{AdminBase}/ECON-001/publication", citizen.Token, new { published = false }))).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/voter/code", new { email = "visitante@example.com" })).StatusCode);
        var voter = await ReadAsync<VoterTokenResponse>(await client.PostAsJsonAsync("/api/v1/voter/verify",
            new { email = "visitante@example.com", code = factory.Emails.LastCodeFor("visitante@example.com") }));
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(Authorized(HttpMethod.Get, AdminBase, voter.Token))).StatusCode);
    }

    [Fact]
    public async Task PapelAdminAvulso_NoIdentity_NaoConcedeAcesso()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        await RegisterAsync(factory, client, "papel@example.com");

        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<AppUser>>();
            await users.AddToRoleAsync((await users.FindByEmailAsync("papel@example.com"))!, "Admin");
        }

        var login = await LoginAsync(client, "papel@example.com");
        Assert.DoesNotContain("Admin", login.Roles);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(Authorized(HttpMethod.Get, AdminBase, login.Token))).StatusCode);
    }

    [Fact]
    public async Task Admin_GetsAdminRoleAndAccess_AndLosesItAsSoonAsTheFlagIsRevoked()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        await RegisterAsync(factory, client, "admin@example.com");
        await SetAdminFlagAsync(factory, "admin@example.com", true);

        var login = await LoginAsync(client, "admin@example.com");
        Assert.Contains("Admin", login.Roles);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(Authorized(HttpMethod.Get, AdminBase, login.Token))).StatusCode);

        var profile = await ReadAsync<ProfileResponse>(await client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/authentication/me", login.Token)));
        Assert.Contains("Admin", profile.Roles);

        await SetAdminFlagAsync(factory, "admin@example.com", false);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(Authorized(HttpMethod.Get, AdminBase, login.Token))).StatusCode);
    }

    [Fact]
    public async Task Admin_ListsAndReadsCommitments_IncludingUnpublished()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        var token = await AdminTokenAsync(factory, client);

        var all = await ReadAsync<PaginedResult<AdminCommitmentSummaryResponse>>(
            await client.SendAsync(Authorized(HttpMethod.Get, $"{AdminBase}?pageSize=100", token)));
        Assert.True(all.TotalCount > 0);
        Assert.All(all.Items, item => Assert.True(item.IsPublished));

        var unpublished = await ReadAsync<AdminCommitmentDetailResponse>(
            await client.SendAsync(Authorized(HttpMethod.Put, $"{AdminBase}/ECON-001/publication", token, new { published = false })));
        Assert.False(unpublished.IsPublished);

        var onlyHidden = await ReadAsync<PaginedResult<AdminCommitmentSummaryResponse>>(
            await client.SendAsync(Authorized(HttpMethod.Get, $"{AdminBase}?published=false", token)));
        Assert.Equal("ECON-001", Assert.Single(onlyHidden.Items).Id);

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/v1/commitments/ECON-001")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await GetAdminAsync(client, token, "ECON-001")).StatusCode);

        var republished = await ReadAsync<AdminCommitmentDetailResponse>(
            await client.SendAsync(Authorized(HttpMethod.Put, $"{AdminBase}/ECON-001/publication", token, new { published = true })));
        Assert.True(republished.IsPublished);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/commitments/ECON-001")).StatusCode);

        Assert.Equal(HttpStatusCode.NotFound, (await GetAdminAsync(client, token, "NAO-EXISTE")).StatusCode);
    }

    [Fact]
    public async Task Admin_UpdatesContentDeadlineAndSources_KeepingARevision()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        var token = await AdminTokenAsync(factory, client);
        var before = await ReadAsync<AdminCommitmentDetailResponse>(await GetAdminAsync(client, token, "ECON-006"));

        var updated = await ReadAsync<AdminCommitmentDetailResponse>(await client.SendAsync(Authorized(HttpMethod.Put, $"{AdminBase}/ECON-006", token,
            ToUpdate(before, "Prazo confirmado no decreto", body =>
            {
                body["title"] = "  Título corrigido  ";
                body["deadlineDate"] = "2028-06-30";
                body["deadlineText"] = "Até 30/06/2028";
                body["shortDescription"] = "   ";
                body["sources"] = new[]
                {
                    new { Name = "Diário Oficial", Url = "https://www.in.gov.br/exemplo", Page = (string?)"12", Kind = "primary" },
                    new { Name = "Jornal", Url = "https://exemplo.com.br/materia", Page = (string?)null, Kind = "press" }
                };
            }))));

        Assert.Equal("Título corrigido", updated.Title);
        Assert.Equal(new DateOnly(2028, 6, 30), updated.DeadlineDate);
        Assert.Equal("Até 30/06/2028", updated.DeadlineText);
        Assert.Null(updated.ShortDescription);
        Assert.Equal(before.Version + 1, updated.Version);
        Assert.Equal(2, updated.Sources.Count);
        Assert.Equal("primary", updated.Sources[0].Kind);

        var revision = Assert.Single(updated.Revisions);
        Assert.Equal(before.Version, revision.Version);
        Assert.Equal("Prazo confirmado no decreto", revision.Reason);
        Assert.Equal("Pessoa Teste", revision.Author);

        var publicView = await ReadAsync<CommitmentDetailResponse>(await client.GetAsync("/api/v1/commitments/ECON-006"));
        Assert.Equal("Título corrigido", publicView.Title);
        Assert.Equal(new DateOnly(2028, 6, 30), publicView.DeadlineDate);
        Assert.Equal(2, publicView.Sources.Count);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True(await context.AuditLogs.AnyAsync(a => a.Action == AuditActions.CommitmentUpdated && a.EntityId == "ECON-006"));
    }

    [Fact]
    public async Task Update_WithAStaleVersion_ReturnsConflict()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        var token = await AdminTokenAsync(factory, client);
        var opened = await ReadAsync<AdminCommitmentDetailResponse>(await GetAdminAsync(client, token, "ECON-006"));

        await ReadAsync<AdminCommitmentDetailResponse>(await client.SendAsync(Authorized(HttpMethod.Put, $"{AdminBase}/ECON-006", token,
            ToUpdate(opened, "Primeira edição", body => body["title"] = "Primeiro título"))));

        var stale = await client.SendAsync(Authorized(HttpMethod.Put, $"{AdminBase}/ECON-006", token,
            ToUpdate(opened, "Segunda edição", body => body["title"] = "Título atropelado")));
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);

        var current = await ReadAsync<AdminCommitmentDetailResponse>(await GetAdminAsync(client, token, "ECON-006"));
        Assert.Equal("Primeiro título", current.Title);
    }

    [Fact]
    public async Task Update_RejectsInvalidInput()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        var token = await AdminTokenAsync(factory, client);
        var c = await ReadAsync<AdminCommitmentDetailResponse>(await GetAdminAsync(client, token, "ECON-006"));

        async Task<HttpStatusCode> Put(string reason, Action<Dictionary<string, object?>> change)
            => (await client.SendAsync(Authorized(HttpMethod.Put, $"{AdminBase}/ECON-006", token, ToUpdate(c, reason, change)))).StatusCode;

        Assert.Equal(HttpStatusCode.BadRequest, await Put("", _ => { }));
        Assert.Equal(HttpStatusCode.BadRequest, await Put("x", b => b["title"] = " "));
        Assert.Equal(HttpStatusCode.BadRequest, await Put("x", b => b["sources"] = Array.Empty<object>()));
        Assert.Equal(HttpStatusCode.BadRequest, await Put("x", b => b["sources"] = new[] { new { Name = "A", Url = "ftp://x", Page = (string?)null, Kind = "press" } }));
        Assert.Equal(HttpStatusCode.BadRequest, await Put("x", b => b["areaCode"] = "XXXX"));
        Assert.Equal(HttpStatusCode.BadRequest, await Put("x", b => b["originCode"] = "nao-existe"));

        var unchanged = await ReadAsync<AdminCommitmentDetailResponse>(await GetAdminAsync(client, token, "ECON-006"));
        Assert.Equal(c.Version, unchanged.Version);
        Assert.Equal(c.Title, unchanged.Title);
    }

    [Fact]
    public async Task RegisterStatus_AppliesTheEditorialRules()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        var token = await AdminTokenAsync(factory, client);

        Task<HttpResponseMessage> Register(object body) =>
            client.SendAsync(Authorized(HttpMethod.Post, $"{AdminBase}/ECON-001/status", token, body));
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);

        Assert.Equal(HttpStatusCode.BadRequest, (await Register(new { status = "Em andamento", referenceDate = yesterday, sourceUrl = (string?)null, isFormalAct = false })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Register(new { status = "Inventada", referenceDate = yesterday, sourceUrl = "https://www.gov.br/x", isFormalAct = false })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Register(new { status = "Descumprido", referenceDate = yesterday, sourceUrl = "https://www.gov.br/x", isFormalAct = false })).StatusCode);

        var inProgress = await ReadAsync<AdminCommitmentDetailResponse>(
            await Register(new { status = "Em andamento", referenceDate = yesterday, sourceUrl = "https://www.gov.br/x", note = "Portaria publicada", isFormalAct = false }));
        Assert.Equal("InProgress", inProgress.Status.Value);
        Assert.Equal("Portaria publicada", inProgress.StatusHistory[0].Note);
        Assert.Equal("Pessoa Teste", inProgress.StatusHistory[0].Author);

        var formal = await ReadAsync<AdminCommitmentDetailResponse>(
            await Register(new { status = "Unfulfilled", referenceDate = yesterday, sourceUrl = "https://www.gov.br/ato", isFormalAct = true }));
        Assert.Equal("Unfulfilled", formal.Status.Value);

        var publicView = await ReadAsync<CommitmentDetailResponse>(await client.GetAsync("/api/v1/commitments/ECON-001"));
        Assert.Equal("Unfulfilled", publicView.Status.Value);
    }

    [Fact]
    public async Task MovingTheDeadline_CannotInvalidateAnExistingUnfulfilled()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        var token = await AdminTokenAsync(factory, client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var c = await ReadAsync<AdminCommitmentDetailResponse>(await GetAdminAsync(client, token, "ECON-001"));
        c = await ReadAsync<AdminCommitmentDetailResponse>(await client.SendAsync(Authorized(HttpMethod.Put, $"{AdminBase}/ECON-001", token,
            ToUpdate(c, "Prazo já vencido", body => body["deadlineDate"] = today.AddDays(-30).ToString("yyyy-MM-dd")))));

        c = await ReadAsync<AdminCommitmentDetailResponse>(await client.SendAsync(Authorized(HttpMethod.Post, $"{AdminBase}/ECON-001/status", token,
            new { status = "Descumprido", referenceDate = today.AddDays(-1), sourceUrl = "https://www.gov.br/x", isFormalAct = false })));

        var later = await client.SendAsync(Authorized(HttpMethod.Put, $"{AdminBase}/ECON-001", token,
            ToUpdate(c, "Adiado", body => body["deadlineDate"] = today.AddDays(90).ToString("yyyy-MM-dd"))));
        Assert.Equal(HttpStatusCode.BadRequest, later.StatusCode);

        var removed = await client.SendAsync(Authorized(HttpMethod.Put, $"{AdminBase}/ECON-001", token,
            ToUpdate(c, "Sem prazo", body => body["deadlineDate"] = null)));
        Assert.Equal(HttpStatusCode.BadRequest, removed.StatusCode);

        var kept = await client.SendAsync(Authorized(HttpMethod.Put, $"{AdminBase}/ECON-001", token,
            ToUpdate(c, "Ajuste de texto", body => body["deadlineDate"] = today.AddDays(-60).ToString("yyyy-MM-dd"))));
        Assert.Equal(HttpStatusCode.OK, kept.StatusCode);
    }

    [Fact]
    public async Task Admin_RenamesAreaAndOrigin_ButOthersCannot()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        var adminToken = await AdminTokenAsync(factory, client);
        await RegisterAsync(factory, client, "cidadao@example.com");
        var citizen = await LoginAsync(client, "cidadao@example.com");

        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/admin/catalog/areas/ECON", citizen.Token, new { name = "Hack" }))).StatusCode);

        var renamed = await ReadAsync<LookupResponse>(await client.SendAsync(
            Authorized(HttpMethod.Put, "/api/v1/admin/catalog/areas/econ", adminToken, new { name = "  Economia e Finanças  " })));
        Assert.Equal("ECON", renamed.Code);
        Assert.Equal("Economia e Finanças", renamed.Name);

        Assert.Equal(HttpStatusCode.NotFound, (await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/admin/catalog/areas/NADA", adminToken, new { name = "x" }))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/admin/catalog/areas/ECON", adminToken, new { name = " " }))).StatusCode);

        var detail = await ReadAsync<CommitmentDetailResponse>(await client.GetAsync("/api/v1/commitments/ECON-001"));
        Assert.Equal("Economia e Finanças", detail.Area.Name);

        var origin = (await ReadAsync<Cobranca.Cidada.Application.Modules.Catalog.Responses.CatalogResponse>(await client.GetAsync("/api/v1/catalog"))).Origins[0];
        var originRenamed = await ReadAsync<LookupResponse>(await client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/catalog/origins/{origin.Code}", adminToken, new { name = "Origem corrigida" })));
        Assert.Equal("Origem corrigida", originRenamed.Name);
    }

    private static Dictionary<string, object?> NewCommitment(Action<Dictionary<string, object?>>? change = null)
    {
        var body = new Dictionary<string, object?>
        {
            ["areaCode"] = "ECON",
            ["originCode"] = "plano-de-governo",
            ["title"] = "Compromisso criado pelo painel",
            ["description"] = "Descrição do novo compromisso.",
            ["hasPlanDetail"] = false,
            ["deadlineDate"] = "2028-12-31",
            ["deadlineText"] = "Até o fim de 2028",
            ["classification"] = new[] { "teste" },
            ["evidence"] = Array.Empty<string>(),
            ["sources"] = new[] { new { Name = "Diário Oficial", Url = "https://www.in.gov.br/novo", Page = (string?)null, Kind = "primary" } },
            ["publish"] = false
        };
        change?.Invoke(body);
        return body;
    }

    [Fact]
    public async Task Admin_CreatesACommitment_UnpublishedWithTheNextIdOfTheArea()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        var token = await AdminTokenAsync(factory, client);

        var highest = (await ReadAsync<PaginedResult<AdminCommitmentSummaryResponse>>(
                await client.SendAsync(Authorized(HttpMethod.Get, $"{AdminBase}?area=ECON&pageSize=100", token))))
            .Items.Select(i => int.Parse(i.Id["ECON-".Length..])).Max();

        var response = await client.SendAsync(Authorized(HttpMethod.Post, AdminBase, token, NewCommitment()));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await ReadAsync<AdminCommitmentDetailResponse>(response);

        Assert.Equal($"ECON-{highest + 1:000}", created.Id);
        Assert.False(created.IsPublished);
        Assert.Equal("NotEvaluated", created.Status.Value);
        Assert.Equal(1, created.Version);
        Assert.Equal(new DateOnly(2028, 12, 31), created.DeadlineDate);
        Assert.Single(created.Sources);

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/commitments/{created.Id}")).StatusCode);
        await ReadAsync<AdminCommitmentDetailResponse>(
            await client.SendAsync(Authorized(HttpMethod.Put, $"{AdminBase}/{created.Id}/publication", token, new { published = true })));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/v1/commitments/{created.Id}")).StatusCode);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True(await context.AuditLogs.AnyAsync(a => a.Action == AuditActions.CommitmentCreated && a.EntityId == created.Id && a.Actor != "seed"));
    }

    [Fact]
    public async Task Create_WithExplicitIdAndPublish_AndRejectsDuplicatesAndInvalidInput()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        var token = await AdminTokenAsync(factory, client);

        Task<HttpResponseMessage> Post(Action<Dictionary<string, object?>> change) =>
            client.SendAsync(Authorized(HttpMethod.Post, AdminBase, token, NewCommitment(change)));

        var created = await ReadAsync<AdminCommitmentDetailResponse>(await Post(b => { b["slug"] = "ECON-900"; b["publish"] = true; }));
        Assert.Equal("ECON-900", created.Id);
        Assert.True(created.IsPublished);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/commitments/ECON-900")).StatusCode);

        Assert.Equal(HttpStatusCode.Conflict, (await Post(b => b["slug"] = "econ-900")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Post(b => b["slug"] = "com espaço")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Post(b => b["title"] = " ")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Post(b => b["sources"] = Array.Empty<object>())).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Post(b => b["areaCode"] = "XXXX")).StatusCode);
    }

    [Fact]
    public async Task Create_IsForbiddenForCitizens()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        await RegisterAsync(factory, client, "cidadao@example.com");
        var citizen = await LoginAsync(client, "cidadao@example.com");

        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(Authorized(HttpMethod.Post, AdminBase, citizen.Token, NewCommitment()))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(Authorized(HttpMethod.Post, AdminBase, null, NewCommitment()))).StatusCode);
    }
}
