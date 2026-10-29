using Cobranca.Cidada.Domain.Interfaces.Services;

namespace Cobranca.Cidada.Infrastructure.Services;

/// <summary>Contadores em memória do processo. Só para desenvolvimento e testes; com várias instâncias use o Redis.</summary>
public class MemoryVoteActivityStore : IVoteActivityStore
{
    private readonly TimeProvider _timeProvider;
    private readonly object _gate = new();
    private readonly Dictionary<string, (long Count, DateTimeOffset ExpiresAt)> _counters = new();
    private readonly Dictionary<string, (HashSet<string> Members, DateTimeOffset ExpiresAt)> _sets = new();
    private readonly Dictionary<string, DateTimeOffset> _flags = new();

    public MemoryVoteActivityStore(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Task<long> IncrementAsync(string key, TimeSpan window, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var now = _timeProvider.GetUtcNow();
            var count = _counters.TryGetValue(key, out var entry) && entry.ExpiresAt > now ? entry.Count + 1 : 1;
            var expires = entry.ExpiresAt > now ? entry.ExpiresAt : now.Add(window);
            _counters[key] = (count, expires);
            return Task.FromResult(count);
        }
    }

    public Task<long> AddDistinctAsync(string key, string member, TimeSpan window, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var now = _timeProvider.GetUtcNow();
            if (!_sets.TryGetValue(key, out var entry) || entry.ExpiresAt <= now)
                entry = ([], now.Add(window));

            entry.Members.Add(member);
            _sets[key] = entry;
            return Task.FromResult((long)entry.Members.Count);
        }
    }

    public Task BlockAsync(string key, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        lock (_gate)
            _flags[key] = _timeProvider.GetUtcNow().Add(duration);
        return Task.CompletedTask;
    }

    public Task<TimeSpan?> GetBlockRemainingAsync(string key, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var now = _timeProvider.GetUtcNow();
            return Task.FromResult<TimeSpan?>(_flags.TryGetValue(key, out var until) && until > now ? until - now : null);
        }
    }

    public Task<bool> TryMarkOnceAsync(string key, TimeSpan window, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var now = _timeProvider.GetUtcNow();
            if (_flags.TryGetValue(key, out var until) && until > now)
                return Task.FromResult(false);

            _flags[key] = now.Add(window);
            return Task.FromResult(true);
        }
    }
}
