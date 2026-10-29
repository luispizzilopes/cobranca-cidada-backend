namespace Cobranca.Cidada.Application.Interfaces.Caching;

public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
}
