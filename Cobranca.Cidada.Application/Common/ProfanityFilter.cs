using Cobranca.Cidada.Domain.Interfaces.Caching;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;

namespace Cobranca.Cidada.Application.Common;

/// <summary>Censura textos com a lista de palavras ativas da tabela <c>ProfanityWords</c> guardada em cache por 10 minutos.</summary>
public class ProfanityFilter
{
    public const string CacheKey = "moderation:profanity-words";
    private static readonly TimeSpan CacheTime = TimeSpan.FromMinutes(10);

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public ProfanityFilter(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<IReadOnlyList<string>> GetWordsAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetAsync<List<string>>(CacheKey, cancellationToken);
        if (cached is not null) return cached;

        var words = (await _unitOfWork.ProfanityWordRepository.GetActiveWordsAsync(cancellationToken)).ToList();
        await _cache.SetAsync(CacheKey, words, CacheTime, cancellationToken);
        return words;
    }

    public async Task<string> CensorAsync(string text, CancellationToken cancellationToken = default)
        => ProfanityCensor.Censor(text, await GetWordsAsync(cancellationToken));
}
