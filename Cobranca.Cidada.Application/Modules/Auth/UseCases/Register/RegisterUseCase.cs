using ErrorOr;
using Cobranca.Cidada.Application.Common;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Auth.Models;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Application.Modules.Auth.Services;
using Cobranca.Cidada.Application.Modules.Auth.UseCases.RequestRegistrationCode;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Modules.User;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.AspNetCore.Identity;

namespace Cobranca.Cidada.Application.Modules.Auth.UseCases.Register;

public class RegisterUseCase : IUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AuthSessionService _authSessionService;
    private readonly IVerificationCodeStore _codeStore;
    private readonly TimeProvider _timeProvider;

    public RegisterUseCase(
        UserManager<AppUser> userManager,
        AuthSessionService authSessionService,
        IVerificationCodeStore codeStore,
        TimeProvider timeProvider)
    {
        _userManager = userManager;
        _authSessionService = authSessionService;
        _codeStore = codeStore;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<LoginResponse>> ExecuteAsync(
        RegisterRequest request,
        SessionMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();

        var outcome = await _codeStore.VerifyAsync(RequestRegistrationCodeUseCase.Purpose, email, request.Code.Trim(), cancellationToken);
        if (outcome != VerificationOutcome.Valid)
            return CodeError(outcome);

        var user = new AppUser
        {
            Name = request.Name.Trim(),
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            ConsentAcceptedAtUtc = _timeProvider.GetUtcNow().UtcDateTime,
            ConsentVersion = PrivacyPolicy.CurrentVersion
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Where(e => !e.Code.StartsWith("Duplicate", StringComparison.Ordinal))
                .Select(e => Error.Validation(e.Code, e.Description))
                .ToList();

            return errors.Count > 0
                ? errors
                : Error.Validation("Auth.RegistrationFailed", "Não foi possível concluir o cadastro.");
        }

        await _userManager.AddToRoleAsync(user, UserRoles.Citizen);

        return await _authSessionService.CreateAsync(user, metadata, cancellationToken);
    }

    internal static Error CodeError(VerificationOutcome outcome) => outcome switch
    {
        VerificationOutcome.Expired => Error.Validation("Auth.CodeExpired", "Código expirado ou inexistente. Solicite um novo código."),
        VerificationOutcome.TooManyAttempts => Error.Custom(429, "Auth.TooManyAttempts", "Muitas tentativas. Aguarde alguns minutos e tente novamente."),
        _ => Error.Validation("Auth.InvalidCode", "Código inválido.")
    };
}
