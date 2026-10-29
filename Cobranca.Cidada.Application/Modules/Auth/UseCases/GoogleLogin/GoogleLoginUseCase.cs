using ErrorOr;
using Cobranca.Cidada.Application.Common;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Auth.Models;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Application.Modules.Auth.Services;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Modules.User;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.AspNetCore.Identity;

namespace Cobranca.Cidada.Application.Modules.Auth.UseCases.GoogleLogin;

/// <summary>
/// Entrar (ou criar a conta) com o Google, sobre o mesmo Identity da conta com senha. A conta Google fica vinculada ao
/// usuário em <c>AspNetUserLogins</c> (provedor "Google" + <c>sub</c>), então entrar por senha ou pelo Google dá no mesmo usuário:
/// mesmos votos, comentários e sessões.
/// <list type="number">
/// <item>Já vinculada: entra no usuário vinculado (vale mesmo que o e-mail do Google tenha mudado).</item>
/// <item>Há conta com o mesmo e-mail: o Google atesta que a pessoa controla a caixa postal, então vincula e entra.
/// A senha continua valendo. Só vale com <c>email_verified</c>; sem isso qualquer um se passaria pelo dono do e-mail.</item>
/// <item>Não há conta: cria sem senha, mas só com o aceite da política de privacidade (LGPD).</item>
/// </list>
/// </summary>
public class GoogleLoginUseCase : IUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IGoogleIdentityVerifier _google;
    private readonly AuthSessionService _authSessionService;
    private readonly TimeProvider _timeProvider;

    public GoogleLoginUseCase(
        UserManager<AppUser> userManager,
        IGoogleIdentityVerifier google,
        AuthSessionService authSessionService,
        TimeProvider timeProvider)
    {
        _userManager = userManager;
        _google = google;
        _authSessionService = authSessionService;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<LoginResponse>> ExecuteAsync(
        GoogleLoginRequest request,
        SessionMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        if (!_google.IsConfigured)
            return Error.Custom(503, "Auth.GoogleUnavailable", "O login com o Google não está disponível no momento.");

        var identity = await _google.VerifyAsync(request.Credential, cancellationToken);
        if (identity is null)
            return InvalidCredential();

        if (!identity.EmailVerified)
            return Error.Unauthorized("Auth.GoogleEmailNotVerified", "O Google não confirmou o e-mail desta conta.");

        var user = await _userManager.FindByLoginAsync(UserLoginProviders.Google, identity.Subject);
        if (user is null)
        {
            var linked = await LinkOrCreateAsync(identity, request.AcceptedPrivacyPolicy);
            if (linked.IsError)
                return linked.Errors;
            user = linked.Value;
        }

        if (!user.Active)
            return InvalidCredential();

        user.LastLoginAt = _timeProvider.GetUtcNow().UtcDateTime;
        await _userManager.UpdateAsync(user);

        return await _authSessionService.CreateAsync(user, metadata, cancellationToken);
    }

    private async Task<ErrorOr<AppUser>> LinkOrCreateAsync(GoogleIdentity identity, bool acceptedPrivacyPolicy)
    {
        var user = await _userManager.FindByEmailAsync(identity.Email);
        if (user is not null)
        {
            if (!user.Active)
                return InvalidCredential();

            var logins = await _userManager.GetLoginsAsync(user);
            if (logins.Any(l => l.LoginProvider == UserLoginProviders.Google))
                return Error.Unauthorized("Auth.GoogleAccountMismatch", "Este e-mail já está vinculado a outra conta Google.");

            var added = await _userManager.AddLoginAsync(user, new UserLoginInfo(UserLoginProviders.Google, identity.Subject, "Google"));
            if (!added.Succeeded)
                return Error.Failure("Auth.GoogleLinkFailed", "Não foi possível vincular a conta Google.");

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            return user;
        }

        if (!acceptedPrivacyPolicy)
            return Error.Validation("Auth.ConsentRequired", "É necessário aceitar a política de privacidade para criar a conta.");

        user = new AppUser
        {
            Name = DisplayName(identity),
            Email = identity.Email,
            UserName = identity.Email,
            EmailConfirmed = true,
            ConsentAcceptedAtUtc = _timeProvider.GetUtcNow().UtcDateTime,
            ConsentVersion = PrivacyPolicy.CurrentVersion
        };

        var created = await _userManager.CreateAsync(user);
        if (!created.Succeeded)
            return Error.Validation("Auth.RegistrationFailed", "Não foi possível concluir o cadastro.");

        await _userManager.AddToRoleAsync(user, UserRoles.Citizen);

        var linkedNew = await _userManager.AddLoginAsync(user, new UserLoginInfo(UserLoginProviders.Google, identity.Subject, "Google"));
        if (!linkedNew.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Error.Failure("Auth.GoogleLinkFailed", "Não foi possível vincular a conta Google.");
        }

        return user;
    }

    private static string DisplayName(GoogleIdentity identity)
    {
        var name = string.IsNullOrWhiteSpace(identity.Name) ? identity.Email.Split('@')[0] : identity.Name.Trim();
        return name.Length <= 100 ? name : name[..100];
    }

    private static Error InvalidCredential()
        => Error.Unauthorized("Auth.InvalidGoogleCredential", "Não foi possível entrar com o Google. Tente de novo.");
}
