using Cobranca.Cidada.Infrastructure.Services;

namespace Cobranca.Cidada.Tests.Infrastructure;

public class MemoryVoteActivityStoreTests
{
    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private static (MemoryVoteActivityStore Store, Clock Clock) Create()
    {
        var clock = new Clock();
        return (new MemoryVoteActivityStore(clock), clock);
    }

    [Fact]
    public async Task Counter_CountsInsideTheWindow_AndRestartsAfterIt()
    {
        var (store, clock) = Create();
        var window = TimeSpan.FromSeconds(10);

        Assert.Equal(1, await store.IncrementAsync("k", window));
        Assert.Equal(2, await store.IncrementAsync("k", window));
        clock.Now = clock.Now.AddSeconds(11);
        Assert.Equal(1, await store.IncrementAsync("k", window));
    }

    [Fact]
    public async Task Counter_WindowIsFixed_NotExtendedByNewActions()
    {
        var (store, clock) = Create();
        var window = TimeSpan.FromSeconds(10);

        await store.IncrementAsync("k", window);
        clock.Now = clock.Now.AddSeconds(6);
        await store.IncrementAsync("k", window);
        clock.Now = clock.Now.AddSeconds(5);

        Assert.Equal(1, await store.IncrementAsync("k", window));
    }

    [Fact]
    public async Task Distinct_CountsEachMemberOnce()
    {
        var (store, _) = Create();
        var window = TimeSpan.FromMinutes(10);

        Assert.Equal(1, await store.AddDistinctAsync("ip", "a", window));
        Assert.Equal(1, await store.AddDistinctAsync("ip", "a", window));
        Assert.Equal(2, await store.AddDistinctAsync("ip", "b", window));
    }

    [Fact]
    public async Task Block_ReportsRemainingTime_AndExpires()
    {
        var (store, clock) = Create();
        Assert.Null(await store.GetBlockRemainingAsync("k"));

        await store.BlockAsync("k", TimeSpan.FromMinutes(5));
        clock.Now = clock.Now.AddMinutes(2);
        Assert.Equal(TimeSpan.FromMinutes(3), await store.GetBlockRemainingAsync("k"));

        clock.Now = clock.Now.AddMinutes(4);
        Assert.Null(await store.GetBlockRemainingAsync("k"));
    }

    [Fact]
    public async Task TryMarkOnce_OnlySucceedsOncePerWindow()
    {
        var (store, clock) = Create();
        var window = TimeSpan.FromHours(1);

        Assert.True(await store.TryMarkOnceAsync("alert", window));
        Assert.False(await store.TryMarkOnceAsync("alert", window));
        clock.Now = clock.Now.AddHours(2);
        Assert.True(await store.TryMarkOnceAsync("alert", window));
    }
}
