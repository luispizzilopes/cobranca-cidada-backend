using Microsoft.Extensions.Configuration;

namespace Cobranca.Cidada.Application.Modules.Seo.Settings;

/// <summary>Endereço público do site (<c>Seo:SiteUrl</c>), base das URLs absolutas do sitemap, do canonical e do Open Graph.</summary>
public class SeoSettings
{
    public SeoSettings(IConfiguration configuration)
    {
        SiteUrl = (configuration["Seo:SiteUrl"] ?? string.Empty).Trim().TrimEnd('/');
    }

    public string SiteUrl { get; }
}
