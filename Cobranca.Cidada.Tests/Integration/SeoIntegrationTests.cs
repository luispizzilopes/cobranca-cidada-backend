using System.Net;
using System.Xml.Linq;
using Cobranca.Cidada.Application.Modules.Commitment.Commands.SeedCommitments;
using Cobranca.Cidada.Infrastructure.Context;
using Cobranca.Cidada.Tests.Application.Commitment;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cobranca.Cidada.Tests.Integration;

[Collection(PostgresCollection.Name)]
public class SeoIntegrationTests
{
    private const string Site = "https://cobranca.example";
    private readonly PostgresFixture _postgres;

    public SeoIntegrationTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    private async Task<ApiFactory> NewFactoryAsync()
    {
        var factory = await _postgres.CreateFactoryAsync(new Dictionary<string, string?> { ["Seo:SiteUrl"] = Site });
        using var scope = factory.Services.CreateScope();
        var seeded = await scope.ServiceProvider.GetRequiredService<IMediator>().Send(
            new SeedCommitmentsCommand(SeedFileLocator.Load(), false, new DateOnly(2027, 1, 5), new DateOnly(2030, 12, 31)));
        Assert.False(seeded.IsError);
        return factory;
    }

    private static async Task<(HttpStatusCode Status, string Body, string? ContentType)> GetAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        return (response.StatusCode, await response.Content.ReadAsStringAsync(), response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Head_OfAPublishedCommitment_CarriesItsTitleCanonicalAndStructuredData()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();

        var (status, body, contentType) = await GetAsync(client, "/api/v1/seo/head?path=/compromissos/ECON-006");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.StartsWith("text/html", contentType);
        Assert.Contains("<title>", body);
        Assert.Contains("(ECON-006) — Cobrança Cidadã</title>", body);
        Assert.Contains($"<link rel=\"canonical\" href=\"{Site}/compromissos/ECON-006\" />", body);
        Assert.Contains("application/ld+json", body);
        Assert.Contains("index,follow", body);
    }

    [Fact]
    public async Task Body_OfAPublishedCommitment_IsASummaryWithoutAnyVoteData()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();

        var (status, body, _) = await GetAsync(client, "/api/v1/seo/body?path=/compromissos/ECON-006");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<h1>", body);
        Assert.Contains("Situação:", body);
        Assert.Contains("Site de cidadãos, sem vínculo com o governo.", body);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/prazos")]
    [InlineData("/entrar")]
    [InlineData("/compromissos/")]
    [InlineData("/compromissos/ECON-006/extra")]
    [InlineData("")]
    public async Task Head_OfOtherRoutes_IsEmptySoTheDefaultTagsApply(string path)
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();

        var (status, body, _) = await GetAsync(client, $"/api/v1/seo/head?path={Uri.EscapeDataString(path)}");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(string.Empty, body);
    }

    [Fact]
    public async Task Head_OfAnUnknownOrUnpublishedCommitment_AsksSearchEnginesNotToIndexIt()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var commitment = await db.Commitments.SingleAsync(c => c.Slug == "ECON-001");
            commitment.Unpublish();
            await db.SaveChangesAsync();
        }

        var unknown = await GetAsync(client, "/api/v1/seo/head?path=/compromissos/NAO-EXISTE-999");
        var unpublished = await GetAsync(client, "/api/v1/seo/head?path=/compromissos/ECON-001");

        foreach (var (status, body, _) in new[] { unknown, unpublished })
        {
            Assert.Equal(HttpStatusCode.OK, status);
            Assert.Contains("noindex", body);
            Assert.DoesNotContain("application/ld+json", body);
        }
        Assert.Equal(string.Empty, (await GetAsync(client, "/api/v1/seo/body?path=/compromissos/ECON-001")).Body);
    }

    [Fact]
    public async Task Sitemap_ListsPublishedCommitmentsOnly_WithAbsoluteUrls()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        int total;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var commitment = await db.Commitments.SingleAsync(c => c.Slug == "ECON-001");
            commitment.Unpublish();
            await db.SaveChangesAsync();
            total = await db.Commitments.CountAsync(c => c.IsPublished);
        }

        var (status, body, contentType) = await GetAsync(client, "/sitemap.xml");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.StartsWith("application/xml", contentType);
        var urls = XDocument.Parse(body).Descendants().Where(e => e.Name.LocalName == "loc").Select(e => e.Value).ToList();
        Assert.All(urls, url => Assert.StartsWith(Site + "/", url));
        Assert.Contains($"{Site}/", urls);
        Assert.Contains($"{Site}/compromissos/ECON-006", urls);
        Assert.DoesNotContain($"{Site}/compromissos/ECON-001", urls);
        Assert.Equal(total, urls.Count(u => u.Contains("/compromissos/")));
    }
}
