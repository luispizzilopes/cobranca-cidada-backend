using System.Text.Json;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Application.Modules.Seo.Services;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Commitment;

namespace Cobranca.Cidada.Tests.Application;

public class SeoPageRendererTests
{
    private const string Site = "https://cobranca.example";

    private static CommitmentDetailResponse Commitment(
        string title = "Isentar quem ganha até R$ 5 mil",
        string description = "Ampliar a faixa de isenção do imposto de renda.",
        string? shortDescription = null,
        IReadOnlyList<SourceResponse>? sources = null) => new(
        "ECON-006", "LULA26-ECON-006", title, description, shortDescription, "Por projeto de lei.",
        new LookupResponse("ECON", "Economia"), new LookupResponse("plano", "Plano de governo"),
        new StatusResponse("NotEvaluated", "Não avaliado"), null, null, null, "até 2026", null, null,
        [], [], "p. 12", true,
        sources ?? [new SourceResponse("Plano de governo", "https://exemplo.gov.br/plano.pdf", "12", "primary")],
        [], 1, new DateTime(2026, 10, 27, 12, 0, 0, DateTimeKind.Utc),
        new VoteSummaryResponse(null, false, null, null, false), null);

    [Fact]
    public void Head_HasTitleDescriptionCanonicalAndSocialTags()
    {
        var head = SeoPageRenderer.CommitmentHead(Commitment(), Site);

        Assert.Contains("<title>Isentar quem ganha até R$ 5 mil (ECON-006) — Cobrança Cidadã</title>", head);
        Assert.Contains("<link rel=\"canonical\" href=\"https://cobranca.example/compromissos/ECON-006\" />", head);
        Assert.Contains("property=\"og:url\" content=\"https://cobranca.example/compromissos/ECON-006\"", head);
        Assert.Contains("property=\"og:image\" content=\"https://cobranca.example/og-image.png\"", head);
        Assert.Contains("name=\"twitter:card\" content=\"summary_large_image\"", head);
        Assert.Contains("content=\"index,follow", head);
        Assert.Contains("Não avaliado · Economia. Ampliar a faixa", head);
    }

    [Fact]
    public void Head_UsesTheShortDescriptionAndKeepsTheSnippetShort()
    {
        var longText = string.Join(' ', Enumerable.Repeat("palavra", 80));

        var head = SeoPageRenderer.CommitmentHead(Commitment(description: longText, shortDescription: "Resumo curto."), Site);
        var snippet = SeoPageRenderer.Description(Commitment(description: longText));

        Assert.Contains("Não avaliado · Economia. Resumo curto.", head);
        Assert.True(snippet.Length <= 160);
        Assert.EndsWith("…", snippet);
    }

    [Fact]
    public void Head_EscapesEverythingThatComesFromTheDatabase()
    {
        var hostile = Commitment(title: "\"><script>alert(1)</script>", description: "<img src=x onerror=alert(1)>");

        var head = SeoPageRenderer.CommitmentHead(hostile, Site);
        var body = SeoPageRenderer.CommitmentBody(hostile);

        Assert.DoesNotContain("<script>alert", head);
        Assert.DoesNotContain("<img", head);
        Assert.DoesNotContain("<script>alert", body);
        Assert.DoesNotContain("<img", body);
        Assert.Contains("&lt;script&gt;", body);
    }

    [Fact]
    public void JsonLd_IsValidJson_AndCannotCloseTheScriptTag()
    {
        var head = SeoPageRenderer.CommitmentHead(Commitment(title: "</script><b>x</b>"), Site);

        var start = head.IndexOf("id=\"seo-jsonld\">", StringComparison.Ordinal) + "id=\"seo-jsonld\">".Length;
        var end = head.IndexOf("</script>", start, StringComparison.Ordinal);
        var json = head[start..end];

        using var document = JsonDocument.Parse(json);
        var graph = document.RootElement.GetProperty("@graph");
        Assert.Equal("WebPage", graph[0].GetProperty("@type").GetString());
        Assert.Equal("BreadcrumbList", graph[1].GetProperty("@type").GetString());
        Assert.Equal("</script><b>x</b>", graph[1].GetProperty("itemListElement")[1].GetProperty("name").GetString());
    }

    [Fact]
    public void Body_SummarizesTheCommitment_KeepsTheIndependenceNotice_AndOnlyLinksHttpSources()
    {
        var body = SeoPageRenderer.CommitmentBody(Commitment(sources:
        [
            new SourceResponse("Plano", "https://exemplo.gov.br/plano.pdf", null, "primary"),
            new SourceResponse("Malicioso", "javascript:alert(1)", null, "press"),
        ]));

        Assert.Contains("<h1>Isentar quem ganha até R$ 5 mil</h1>", body);
        Assert.Contains("Situação: Não avaliado", body);
        Assert.Contains("Site de cidadãos, sem vínculo com o governo.", body);
        Assert.Contains("href=\"https://exemplo.gov.br/plano.pdf\"", body);
        Assert.DoesNotContain("javascript:", body);
    }

    [Fact]
    public void Body_NeverCarriesVoteCounts()
    {
        var body = SeoPageRenderer.CommitmentBody(Commitment());

        Assert.DoesNotContain("cobrança", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("voto", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sitemap_ListsTheStaticPagesAndEachCommitmentWithItsLastModification()
    {
        var xml = SeoPageRenderer.Sitemap(Site,
        [
            new PublishedCommitmentRef("ECON-006", new DateTime(2026, 10, 27, 12, 0, 0, DateTimeKind.Utc)),
            new PublishedCommitmentRef("SAUD-001", new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc)),
        ]);

        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", xml);
        Assert.Contains("<loc>https://cobranca.example/</loc>", xml);
        Assert.Contains("<loc>https://cobranca.example/prazos</loc>", xml);
        Assert.Contains("<loc>https://cobranca.example/compromissos/ECON-006</loc><lastmod>2026-10-27</lastmod>", xml);
        Assert.Contains("<loc>https://cobranca.example/compromissos/SAUD-001</loc><lastmod>2026-09-01</lastmod>", xml);
        Assert.DoesNotContain("/entrar", xml);
        Assert.DoesNotContain("/conta", xml);
        Assert.DoesNotContain("/admin", xml);
    }
}
