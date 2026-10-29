using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Domain.Interfaces.Caching;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Application.Modules.Seo.Services;
using Cobranca.Cidada.Application.Modules.Seo.Settings;

namespace Cobranca.Cidada.Application.Modules.Seo.UseCases.GetSitemap;

/// <summary>Sitemap com as páginas estáticas e todos os compromissos publicados (lido do banco; despublicar tira da lista).</summary>
public class GetSitemapUseCase : IUseCase
{
    private const string CacheKey = "seo:sitemap";
    private static readonly TimeSpan CacheTime = TimeSpan.FromMinutes(10);

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly SeoSettings _settings;

    public GetSitemapUseCase(IUnitOfWork unitOfWork, ICacheService cache, SeoSettings settings)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _settings = settings;
    }

    public async Task<ErrorOr<string>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetAsync<string>(CacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var refs = await _unitOfWork.CommitmentRepository.GetPublishedRefsAsync(cancellationToken);
        var xml = SeoPageRenderer.Sitemap(_settings.SiteUrl, refs);

        await _cache.SetAsync(CacheKey, xml, CacheTime, cancellationToken);
        return xml;
    }
}
