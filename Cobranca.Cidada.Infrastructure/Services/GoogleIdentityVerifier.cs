using Cobranca.Cidada.Domain.Interfaces.Services;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace Cobranca.Cidada.Infrastructure.Services;

/// <summary>
/// Valida o ID token com a biblioteca oficial do Google: confere a assinatura com as chaves públicas dele
/// (em cache), o emissor, a validade e se o token foi emitido para o nosso <c>ClientId</c> (público).
/// O token vem do navegador, então nada nele é confiável antes dessa validação.
/// </summary>
public class GoogleIdentityVerifier : IGoogleIdentityVerifier
{
    private readonly string? _clientId;

    public GoogleIdentityVerifier(IConfiguration configuration)
    {
        var value = configuration["Authentication:Google:ClientId"];
        _clientId = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public bool IsConfigured => _clientId is not null;

    public async Task<GoogleIdentity?> VerifyAsync(string credential, CancellationToken cancellationToken = default)
    {
        if (_clientId is null || string.IsNullOrWhiteSpace(credential))
            return null;

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                credential,
                new GoogleJsonWebSignature.ValidationSettings { Audience = [_clientId] });

            if (string.IsNullOrWhiteSpace(payload.Subject) || string.IsNullOrWhiteSpace(payload.Email))
                return null;

            var issuedAt = payload.IssuedAtTimeSeconds is { } iat
                ? DateTimeOffset.FromUnixTimeSeconds(iat)
                : DateTimeOffset.MinValue;

            return new GoogleIdentity(payload.Subject, payload.Email.Trim(), payload.EmailVerified, payload.Name, issuedAt);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}
