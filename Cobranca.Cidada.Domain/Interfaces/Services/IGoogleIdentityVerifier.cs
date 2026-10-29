namespace Cobranca.Cidada.Domain.Interfaces.Services;

/// <summary>Dados confiáveis de um ID token do Google, já validado (assinatura, emissor, validade e público).</summary>
/// <param name="Subject">Identificador estável da conta Google (claim <c>sub</c>). O e-mail pode mudar; este não.</param>
/// <param name="EmailVerified">O Google garante que a pessoa controla a caixa postal.</param>
public record GoogleIdentity(string Subject, string Email, bool EmailVerified, string? Name, DateTimeOffset IssuedAtUtc);

/// <summary>Valida o ID token emitido pelo "Entrar com o Google" (Google Identity Services).</summary>
public interface IGoogleIdentityVerifier
{
    /// <summary>Falso quando <c>Authentication:Google:ClientId</c> não está configurado: o login com Google fica indisponível.</summary>
    bool IsConfigured { get; }

    /// <summary>Devolve a identidade, ou <c>null</c> se o token for inválido, expirado ou de outro aplicativo.</summary>
    Task<GoogleIdentity?> VerifyAsync(string credential, CancellationToken cancellationToken = default);
}
