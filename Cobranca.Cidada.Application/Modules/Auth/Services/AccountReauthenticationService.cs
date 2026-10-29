using ErrorOr;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Modules.User;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.AspNetCore.Identity;

namespace Cobranca.Cidada.Application.Modules.Auth.Services;

/// <summary>
/// Confirma que quem está na sessão é mesmo o dono da conta antes de ações sensíveis (apagar a conta, trocar o e-mail).
/// Com senha, confere a senha. Quem entra só com o Google não tem senha: precisa apresentar um ID token do Google
/// recém-emitido e que pertença à conta Google vinculada a este usuário.
/// </summary>
public class AccountReauthenticationService(UserManager<AppUser> userManager, IGoogleIdentityVerifier google)
{
    /// <summary>Um token mais velho que isso não prova presença agora: a pessoa precisa confirmar de novo.</summary>
    private static readonly TimeSpan MaxGoogleCredentialAge = TimeSpan.FromMinutes(10);

    public async Task<ErrorOr<Success>> ConfirmAsync(AppUser user, string? password, string? googleCredential, CancellationToken cancellationToken)
    {
        if (await userManager.HasPasswordAsync(user))
        {
            return !string.IsNullOrEmpty(password) && await userManager.CheckPasswordAsync(user, password)
                ? Result.Success
                : Error.Unauthorized("Auth.InvalidCredentials", "Senha incorreta.");
        }

        if (string.IsNullOrWhiteSpace(googleCredential))
            return Error.Validation("Auth.GoogleConfirmationRequired", "Sua conta não tem senha. Confirme com o Google para continuar.");

        var identity = await google.VerifyAsync(googleCredential, cancellationToken);
        if (identity is null || DateTimeOffset.UtcNow - identity.IssuedAtUtc > MaxGoogleCredentialAge)
            return Error.Unauthorized("Auth.InvalidGoogleCredential", "Não foi possível confirmar com o Google. Tente de novo.");

        var logins = await userManager.GetLoginsAsync(user);
        return logins.Any(l => l.LoginProvider == UserLoginProviders.Google && l.ProviderKey == identity.Subject)
            ? Result.Success
            : Error.Unauthorized("Auth.InvalidGoogleCredential", "Esta conta Google não é a vinculada à sua conta.");
    }
}
