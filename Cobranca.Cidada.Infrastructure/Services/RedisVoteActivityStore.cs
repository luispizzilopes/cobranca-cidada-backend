using Cobranca.Cidada.Domain.Interfaces.Services;
using StackExchange.Redis;

namespace Cobranca.Cidada.Infrastructure.Services;

/// <summary>Contadores atômicos no Redis (INCR/SADD com expiração), compartilhados entre as instâncias da API.</summary>
public class RedisVoteActivityStore : IVoteActivityStore
{
    private const string Prefix = "burst:";
    private readonly IConnectionMultiplexer _redis;

    public RedisVoteActivityStore(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<long> IncrementAsync(string key, TimeSpan window, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var count = await db.StringIncrementAsync(Prefix + key);
        if (count == 1)
            await db.KeyExpireAsync(Prefix + key, window);
        return count;
    }

    public async Task<long> AddDistinctAsync(string key, string member, TimeSpan window, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.SetAddAsync(Prefix + key, member);
        if (await db.KeyTimeToLiveAsync(Prefix + key) is null)
            await db.KeyExpireAsync(Prefix + key, window);
        return await db.SetLengthAsync(Prefix + key);
    }

    public async Task BlockAsync(string key, TimeSpan duration, CancellationToken cancellationToken = default)
        => await _redis.GetDatabase().StringSetAsync(Prefix + "block:" + key, "1", duration);

    public async Task<TimeSpan?> GetBlockRemainingAsync(string key, CancellationToken cancellationToken = default)
        => await _redis.GetDatabase().KeyTimeToLiveAsync(Prefix + "block:" + key);

    public async Task<bool> TryMarkOnceAsync(string key, TimeSpan window, CancellationToken cancellationToken = default)
        => await _redis.GetDatabase().StringSetAsync(Prefix + "once:" + key, "1", window, When.NotExists);
}
