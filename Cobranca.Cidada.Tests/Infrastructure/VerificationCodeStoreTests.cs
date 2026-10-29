using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Infrastructure.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Cobranca.Cidada.Tests.Infrastructure;

public class VerificationCodeStoreTests
{
    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private static (VerificationCodeStore Store, Clock Clock) Create()
    {
        var clock = new Clock();
        IDistributedCache cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        return (new VerificationCodeStore(cache, new MemoryVoteActivityStore(clock), clock), clock);
    }

    [Fact]
    public async Task CorrectCode_IsValidOnlyOnce()
    {
        var (store, _) = Create();
        await store.StoreAsync("voter", "a@example.com", "123456");

        Assert.Equal(VerificationOutcome.Valid, await store.VerifyAsync("voter", "a@example.com", "123456"));
        Assert.Equal(VerificationOutcome.Expired, await store.VerifyAsync("voter", "a@example.com", "123456"));
    }

    [Fact]
    public async Task WrongCode_IsInvalid_AndLocksAfterFiveAttempts_EvenWithTheRightCode()
    {
        var (store, _) = Create();
        await store.StoreAsync("voter", "a@example.com", "123456");

        for (var i = 0; i < 4; i++)
            Assert.Equal(VerificationOutcome.Invalid, await store.VerifyAsync("voter", "a@example.com", "000000"));

        Assert.Equal(VerificationOutcome.TooManyAttempts, await store.VerifyAsync("voter", "a@example.com", "000000"));
        Assert.Equal(VerificationOutcome.TooManyAttempts, await store.VerifyAsync("voter", "a@example.com", "123456"));
    }

    [Fact]
    public async Task Attempts_AreNotResetByRequestingANewCode()
    {
        var (store, _) = Create();
        await store.StoreAsync("voter", "a@example.com", "123456");
        for (var i = 0; i < 5; i++)
            await store.VerifyAsync("voter", "a@example.com", "000000");

        await store.StoreAsync("voter", "a@example.com", "654321");

        Assert.Equal(VerificationOutcome.TooManyAttempts, await store.VerifyAsync("voter", "a@example.com", "654321"));
    }

    [Fact]
    public async Task ParallelGuesses_CannotExceedTheAttemptLimit()
    {
        var (store, _) = Create();
        await store.StoreAsync("voter", "a@example.com", "123456");

        var outcomes = await Task.WhenAll(Enumerable.Range(0, 50)
            .Select(_ => Task.Run(() => store.VerifyAsync("voter", "a@example.com", "000000"))));

        Assert.Equal(4, outcomes.Count(o => o == VerificationOutcome.Invalid));
        Assert.Equal(VerificationOutcome.TooManyAttempts, await store.VerifyAsync("voter", "a@example.com", "123456"));
    }

    [Fact]
    public async Task Code_ExpiresAfterFifteenMinutes()
    {
        var (store, clock) = Create();
        await store.StoreAsync("voter", "a@example.com", "123456");

        clock.Now = clock.Now.AddMinutes(16);

        Assert.Equal(VerificationOutcome.Expired, await store.VerifyAsync("voter", "a@example.com", "123456"));
    }

    [Fact]
    public async Task Code_IsBoundToPurposeAndMailbox()
    {
        var (store, _) = Create();
        await store.StoreAsync("voter", "a@example.com", "123456");

        Assert.Equal(VerificationOutcome.Expired, await store.VerifyAsync("registration", "a@example.com", "123456"));
        Assert.Equal(VerificationOutcome.Expired, await store.VerifyAsync("voter", "b@example.com", "123456"));
    }

    [Fact]
    public async Task Resend_IsBlockedWithinTheCooldown()
    {
        var (store, _) = Create();

        Assert.True(await store.TryReserveSendAsync("voter", "a@example.com"));
        Assert.False(await store.TryReserveSendAsync("voter", "a@example.com"));
        Assert.True(await store.TryReserveSendAsync("voter", "outra@example.com"));
    }
}
