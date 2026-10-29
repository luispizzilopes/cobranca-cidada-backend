using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Commitment;

namespace Cobranca.Cidada.Application.Modules.Seo.Services;

/// <summary>
/// Monta o HTML que o nginx injeta no <c>index.html</c> (SSI) e o sitemap. Funções puras, sem I/O.
/// Tudo que vem do banco passa por <see cref="E"/> (ou pelo serializador JSON, nos dados
/// estruturados): o conteúdo é editado por curadores, mas nunca entra no HTML sem escape.
/// </summary>
public static class SeoPageRenderer
{
    public const string SiteName = "Cobrança Cidadã";
    public const string IndependentNotice = "Site de cidadãos, sem vínculo com o governo.";
    public const string DefaultImagePath = "/og-image.png";

    /// <summary>Páginas estáticas indexáveis do site (as demais rotas públicas são de conta e ficam fora do índice).</summary>
    public static readonly string[] StaticPaths = ["/", "/prazos", "/metodologia", "/limitacoes", "/privacidade"];

    public static string CommitmentPath(string slug) => $"/compromissos/{Uri.EscapeDataString(slug)}";

    public static string Title(CommitmentDetailResponse c) => $"{c.Title} ({c.Id}) — {SiteName}";

    public static string Description(CommitmentDetailResponse c)
    {
        var text = string.IsNullOrWhiteSpace(c.ShortDescription) ? c.Description : c.ShortDescription;
        return Truncate($"{c.Status.Label} · {c.Area.Name}. {Collapse(text)}", 160);
    }

    /// <summary>Etiquetas do <c>&lt;head&gt;</c> de um compromisso publicado. Substitui as padrão do <c>index.html</c>.</summary>
    public static string CommitmentHead(CommitmentDetailResponse c, string siteUrl)
    {
        var url = siteUrl + CommitmentPath(c.Id);
        var title = Title(c);
        var description = Description(c);
        var image = siteUrl + DefaultImagePath;

        var sb = new StringBuilder();
        sb.AppendLine($"<title>{E(title)}</title>");
        sb.AppendLine($"<meta name=\"description\" content=\"{E(description)}\" />");
        sb.AppendLine("<meta name=\"robots\" content=\"index,follow,max-image-preview:large\" />");
        sb.AppendLine($"<link rel=\"canonical\" href=\"{E(url)}\" />");
        sb.AppendLine($"<meta property=\"og:site_name\" content=\"{E(SiteName)}\" />");
        sb.AppendLine("<meta property=\"og:locale\" content=\"pt_BR\" />");
        sb.AppendLine("<meta property=\"og:type\" content=\"article\" />");
        sb.AppendLine($"<meta property=\"og:title\" content=\"{E(title)}\" />");
        sb.AppendLine($"<meta property=\"og:description\" content=\"{E(description)}\" />");
        sb.AppendLine($"<meta property=\"og:url\" content=\"{E(url)}\" />");
        sb.AppendLine($"<meta property=\"og:image\" content=\"{E(image)}\" />");
        sb.AppendLine("<meta name=\"twitter:card\" content=\"summary_large_image\" />");
        sb.AppendLine($"<meta name=\"twitter:title\" content=\"{E(title)}\" />");
        sb.AppendLine($"<meta name=\"twitter:description\" content=\"{E(description)}\" />");
        sb.AppendLine($"<meta name=\"twitter:image\" content=\"{E(image)}\" />");
        sb.AppendLine($"<script type=\"application/ld+json\" id=\"seo-jsonld\">{JsonLd(c, url, title, description, siteUrl)}</script>");
        return sb.ToString();
    }

    /// <summary>
    /// Resumo em HTML simples para dentro de <c>#root</c>: quem não executa JavaScript (buscadores simples, prévia de
    /// links, leitores) vê o conteúdo. O React o substitui assim que carrega.
    /// </summary>
    public static string CommitmentBody(CommitmentDetailResponse c)
    {
        var sb = new StringBuilder();
        sb.Append("<div id=\"seo-prerender\"><article>");
        sb.Append($"<p>{E(IndependentNotice)}</p>");
        sb.Append($"<h1>{E(c.Title)}</h1>");
        sb.Append($"<p>{E(c.Id)} · {E(c.Area.Name)} · Situação: {E(c.Status.Label)}</p>");
        sb.Append($"<p>{E(c.Description)}</p>");
        if (!string.IsNullOrWhiteSpace(c.HowItWillBeDone))
            sb.Append($"<h2>Como será feito</h2><p>{E(c.HowItWillBeDone)}</p>");
        if (!string.IsNullOrWhiteSpace(c.DeadlineText))
            sb.Append($"<h2>Prazo</h2><p>{E(c.DeadlineText)}</p>");
        if (c.Sources.Count > 0)
        {
            sb.Append("<h2>Fontes</h2><ul>");
            foreach (var source in c.Sources.Where(s => IsHttpUrl(s.Url)))
                sb.Append($"<li><a href=\"{E(source.Url)}\" rel=\"noopener nofollow\">{E(source.Name)}</a></li>");
            sb.Append("</ul>");
        }
        sb.Append("<p><a href=\"/\">Ver todos os compromissos</a></p>");
        sb.Append("</article></div>");
        return sb.ToString();
    }

    /// <summary>Compromisso inexistente ou despublicado: a página responde 200 (é uma SPA), então avisa os buscadores para não indexar.</summary>
    public static string NotFoundHead() =>
        $"<title>Compromisso não encontrado — {E(SiteName)}</title>\n<meta name=\"robots\" content=\"noindex,nofollow\" />\n";

    public static string Sitemap(string siteUrl, IEnumerable<PublishedCommitmentRef> commitments)
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urls = StaticPaths
            .Select(path => new XElement(ns + "url", new XElement(ns + "loc", siteUrl + (path == "/" ? "/" : path))))
            .Concat(commitments.Select(c => new XElement(ns + "url",
                new XElement(ns + "loc", siteUrl + CommitmentPath(c.Slug)),
                new XElement(ns + "lastmod", c.UpdatedAtUtc.ToString("yyyy-MM-dd")))));

        var document = new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement(ns + "urlset", urls));
        return document.Declaration + "\n" + document.Root!.ToString(SaveOptions.DisableFormatting);
    }

    private static string JsonLd(CommitmentDetailResponse c, string url, string title, string description, string siteUrl)
    {
        var graph = new object[]
        {
            new Dictionary<string, object?>
            {
                ["@type"] = "WebPage",
                ["@id"] = url,
                ["url"] = url,
                ["name"] = title,
                ["description"] = description,
                ["inLanguage"] = "pt-BR",
                ["dateModified"] = c.UpdatedAtUtc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
                ["isPartOf"] = new Dictionary<string, object?> { ["@type"] = "WebSite", ["name"] = SiteName, ["url"] = siteUrl + "/" },
            },
            new Dictionary<string, object?>
            {
                ["@type"] = "BreadcrumbList",
                ["itemListElement"] = new object[]
                {
                    new Dictionary<string, object?> { ["@type"] = "ListItem", ["position"] = 1, ["name"] = "Compromissos", ["item"] = siteUrl + "/" },
                    new Dictionary<string, object?> { ["@type"] = "ListItem", ["position"] = 2, ["name"] = c.Title, ["item"] = url },
                },
            },
        };

        return JsonSerializer.Serialize(new Dictionary<string, object?> { ["@context"] = "https://schema.org", ["@graph"] = graph });
    }

    private static bool IsHttpUrl(string value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    /// <summary>
    /// Escapa só o que muda o sentido do HTML (&amp; &lt; &gt; aspas). Diferente do <c>WebUtility.HtmlEncode</c>,
    /// mantém acentos como estão: o texto é em português e a página é UTF-8.
    /// </summary>
    private static string E(string? value)
        => (value ?? string.Empty)
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&#39;", StringComparison.Ordinal);

    private static string Collapse(string value) => string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..(max - 1)].TrimEnd() + "…";
}
