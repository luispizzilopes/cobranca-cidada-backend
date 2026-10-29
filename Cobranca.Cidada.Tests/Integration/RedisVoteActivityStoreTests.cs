using Cobranca.Cidada.Infrastructure.Services;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace Cobranca.Cidada.Tests.Integration;

/// <summary>O caminho de produção dos contadores de rajada, contra um Redis real.</summary>
public class RedisVoteActivityStoreTests : IAsyncLifetime
{
    private readonly RedisContainer _container = new RedisBuilder().WithImage("redis:7-alpine").Build();
    private IConnectionMultiplexer _redis = null!;
    private RedisVoteActivityStore _store = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _redis = await ConnectionMultiplexer.ConnectAsync(_container.GetConnectionString());
        _store = new RedisVoteActivityStore(_redis);
    }

    public async Task DisposeAsync()
    {
        await _redis.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task Counter_IsAtomicAcrossConcurrentCallers_AndExpires()
    {
        var results = await Task.WhenAll(Enumerable.Range(0, 50).Select(_ => _store.IncrementAsync("voter", TimeSpan.FromSeconds(2))));

        Assert.Equal(Enumerable.Range(1, 50).Select(i => (long)i), results.OrderBy(x => x));

        await Task.Delay(TimeSpan.FromSeconds(2.5));
        Assert.Equal(1, await _store.IncrementAsync("voter", TimeSpan.FromSeconds(2)));
    }

    [Fact]
    public async Task Distinct_CountsEachMemberOnce_AndExpires()
    {
        Assert.Equal(1, await _store.AddDistinctAsync("ip", "a", TimeSpan.FromSeconds(2)));
        Assert.Equal(1, await _store.AddDistinctAsync("ip", "a", TimeSpan.FromSeconds(2)));
        Assert.Equal(2, await _store.AddDistinctAsync("ip", "b", TimeSpan.FromSeconds(2)));

        await Task.Delay(TimeSpan.FromSeconds(2.5));
        Assert.Equal(1, await _store.AddDistinctAsync("ip", "c", TimeSpan.FromSeconds(2)));
    }

    [Fact]
    public async Task Block_ReportsRemainingTime_AndExpires()
    {
        Assert.Null(await _store.GetBlockRemainingAsync("k"));

        await _store.BlockAsync("k", TimeSpan.FromSeconds(2));
        var remaining = await _store.GetBlockRemainingAsync("k");
        Assert.NotNull(remaining);
        Assert.InRange(remaining!.Value.TotalSeconds, 0.1, 2);

        await Task.Delay(TimeSpan.FromSeconds(2.5));
        Assert.Null(await _store.GetBlockRemainingAsync("k"));
    }

    [Fact]
    public async Task TryMarkOnce_OnlySucceedsOncePerWindow()
    {
        Assert.True(await _store.TryMarkOnceAsync("alert", TimeSpan.FromSeconds(2)));
        Assert.False(await _store.TryMarkOnceAsync("alert", TimeSpan.FromSeconds(2)));

        await Task.Delay(TimeSpan.FromSeconds(2.5));
        Assert.True(await _store.TryMarkOnceAsync("alert", TimeSpan.FromSeconds(2)));
    }
}
