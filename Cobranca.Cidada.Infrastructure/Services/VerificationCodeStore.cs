using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Cobranca.Cidada.Infrastructure.Services;

public class VerificationCodeStore : IVerificationCodeStore
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ResendInterval = TimeSpan.FromSeconds(60);

    private readonly IDistributedCache _cache;
    private readonly IVoteActivityStore _counters;
    private readonly TimeProvider _timeProvider;

    public VerificationCodeStore(IDistributedCache cache, IVoteActivityStore counters, TimeProvider timeProvider)
    {
        _cache = cache;
        _counters = counters;
        _timeProvider = timeProvider;
    }

    public async Task<bool> TryReserveSendAsync(string purpose, string email, CancellationToken cancellationToken = default)
    {
        var key = Key("cooldown", purpose, email);
        if (await _cache.GetStringAsync(key, cancellationToken) is not null)
            return false;

        await _cache.SetStringAsync(key, "1", new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ResendInterval }, cancellationToken);
        return true;
    }

    public async Task StoreAsync(string purpose, string email, string code, CancellationToken cancellationToken = default)
    {
        var entry = new Entry(Hash(purpose, email, code), _timeProvider.GetUtcNow().Add(CodeLifetime));
        await _cache.SetStringAsync(
            Key("code", purpose, email),
            JsonSerializer.Serialize(entry),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CodeLifetime },
            cancellationToken);
    }

    public async Task<VerificationOutcome> VerifyAsync(string purpose, string email, string code, CancellationToken cancellationToken = default)
    {
        var key = Key("code", purpose, email);
        var raw = await _cache.GetStringAsync(key, cancellationToken);
        if (raw is null)
            return VerificationOutcome.Expired;

        var entry = JsonSerializer.Deserialize<Entry>(raw)!;
        if (entry.ExpiresAt <= _timeProvider.GetUtcNow())
        {
            await _cache.RemoveAsync(key, cancellationToken);
            return VerificationOutcome.Expired;
        }

        var attempts = await _counters.IncrementAsync(Key("attempts", purpose, email), CodeLifetime, cancellationToken);
        if (attempts > MaxAttempts)
            return VerificationOutcome.TooManyAttempts;

        var matches = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(entry.CodeHash),
            Encoding.UTF8.GetBytes(Hash(purpose, email, code)));

        if (matches)
        {
            await _cache.RemoveAsync(key, cancellationToken);
            return VerificationOutcome.Valid;
        }

        return attempts >= MaxAttempts ? VerificationOutcome.TooManyAttempts : VerificationOutcome.Invalid;
    }

    public async Task RemoveAsync(string purpose, string email, CancellationToken cancellationToken = default)
        => await _cache.RemoveAsync(Key("code", purpose, email), cancellationToken);

    private static string Key(string kind, string purpose, string email)
        => $"{kind}:{purpose}:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(EmailNormalizer.Canonicalize(email)))).ToLowerInvariant()}";

    private static string Hash(string purpose, string email, string code)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{purpose}|{EmailNormalizer.Canonicalize(email)}|{code}")));

    private record Entry(string CodeHash, DateTimeOffset ExpiresAt);
}
