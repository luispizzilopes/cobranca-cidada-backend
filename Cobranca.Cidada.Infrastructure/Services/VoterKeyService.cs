using System.Security.Cryptography;
using System.Text;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Cobranca.Cidada.Infrastructure.Services;

public class VoterKeyService : IVoterKeyService
{
    private readonly byte[] _key;

    public VoterKeyService(IConfiguration configuration)
    {
        var secret = configuration["Voting:EmailHmacKey"];
        if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32)
            throw new InvalidOperationException("Voting:EmailHmacKey deve ter ao menos 32 caracteres.");

        _key = Encoding.UTF8.GetBytes(secret);
    }

    public string Compute(string email)
        => Convert.ToHexString(HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(EmailNormalizer.Canonicalize(email)))).ToLowerInvariant();

    public string Pseudonymize(string scope, string value)
        => Convert.ToHexString(HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes($"{scope}|{value}"))).ToLowerInvariant();
}
