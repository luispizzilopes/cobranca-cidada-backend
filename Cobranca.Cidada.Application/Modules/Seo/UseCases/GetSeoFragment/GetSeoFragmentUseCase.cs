using System.Text.RegularExpressions;
using ErrorOr;
using Cobranca.Cidada.Application.Common;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Application.Modules.Seo.Queries.GetSeoFragment;
using Cobranca.Cidada.Application.Modules.Seo.Services;
using Cobranca.Cidada.Domain.Interfaces.Caching;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;

namespace Cobranca.Cidada.Application.Modules.Seo.UseCases.GetSeoFragment;

/// <summary>
/// O nginx chama este caso de uso a cada carregamento de página (include SSI). Só o detalhe de um compromisso tem conteúdo
/// próprio: as demais rotas devolvem vazio e valem as etiquetas padrão do <c>index.html</c> (ou da cópia pré-gerada da página).
/// O resultado fica em cache por pouco tempo para o banco não ser consultado a cada visita.
/// </summary>
public partial class GetSeoFragmentUseCase : IUseCase
{
    private static readonly TimeSpan CacheTime = TimeSpan.FromSeconds(60);

    [GeneratedRegex(@"^/compromissos/(?<slug>[A-Za-z0-9._-]{1,120})/?$")]
    private static partial Regex CommitmentRoute();

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly SeoSettings _settings;

    public GetSeoFragmentUseCase(IUnitOfWork unitOfWork, ICacheService cache, SeoSettings settings)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _settings = settings;
    }

    public async Task<ErrorOr<string>> ExecuteAsync(string? path, SeoFragmentPart part, CancellationToken cancellationToken = default)
    {
        var match = CommitmentRoute().Match(path?.Split('?')[0] ?? string.Empty);
        if (!match.Success)
            return string.Empty;

        var slug = match.Groups["slug"].Value;
        var cacheKey = $"seo:{part}:{slug}";
        var cached = await _cache.GetAsync<string>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var commitment = await _unitOfWork.CommitmentRepository.GetBySlugAsync(slug, onlyPublished: true, cancellationToken);
        if (commitment is null)
            return part == SeoFragmentPart.Head ? SeoPageRenderer.NotFoundHead() : string.Empty;

        var detail = commitment.ToDetail(new VoteSummaryResponse(null, false, null, null, false), null);
        var html = part == SeoFragmentPart.Head
            ? SeoPageRenderer.CommitmentHead(detail, _settings.SiteUrl)
            : SeoPageRenderer.CommitmentBody(detail);

        await _cache.SetAsync(cacheKey, html, CacheTime, cancellationToken);
        return html;
    }
}
