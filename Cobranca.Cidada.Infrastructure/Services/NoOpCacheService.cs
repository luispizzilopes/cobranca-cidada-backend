using Cobranca.Cidada.Domain.Interfaces.Caching;

namespace Cobranca.Cidada.Infrastructure.Services;

/// <summary>Cache nulo, usado quando o Redis não está configurado (ex.: testes e execução local mínima).</summary>
public class NoOpCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) => Task.FromResult(default(T));
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult(false);
    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<long> GetUsedMemoryBytesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0L);
}
