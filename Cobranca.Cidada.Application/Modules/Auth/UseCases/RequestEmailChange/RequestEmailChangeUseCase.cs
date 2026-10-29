using System.Security.Cryptography;
using ErrorOr;
using Cobranca.Cidada.Application.Common.Emails;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using Cobranca.Cidada.Application.Modules.Auth.Services;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.AspNetCore.Identity;

namespace Cobranca.Cidada.Application.Modules.Auth.UseCases.RequestEmailChange;

public class RequestEmailChangeUseCase : IUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IVerificationCodeStore _codeStore;
    private readonly IVoterKeyService _voterKeyService;
    private readonly AccountReauthenticationService _reauthentication;

    public RequestEmailChangeUseCase(
        UserManager<AppUser> userManager,
        IEmailService emailService,
        IVerificationCodeStore codeStore,
        IVoterKeyService voterKeyService,
        AccountReauthenticationService reauthentication)
    {
        _reauthentication = reauthentication;
        _userManager = userManager;
        _emailService = emailService;
        _codeStore = codeStore;
        _voterKeyService = voterKeyService;
    }

    public static string PurposeFor(string userId) => $"email-change:{userId}";

    public async Task<ErrorOr<Success>> ExecuteAsync(string userId, RequestEmailChangeRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Error.NotFound("Auth.UserNotFound", "Usuário não encontrado.");

        var confirmed = await _reauthentication.ConfirmAsync(user, request.Password, request.GoogleCredential, cancellationToken);
        if (confirmed.IsError)
            return confirmed.Errors;

        var newEmail = request.NewEmail.Trim();

        if (_voterKeyService.Compute(newEmail) == _voterKeyService.Compute(user.Email!))
            return Error.Validation("Auth.SameEmail", "Informe um e-mail diferente do atual.");

        var purpose = PurposeFor(user.Id);
        if (!await _codeStore.TryReserveSendAsync(purpose, newEmail, cancellationToken))
            return Error.Custom(429, "Auth.TooManyRequests", "Aguarde um minuto antes de pedir um novo código.");

        try
        {
            var taken = await _userManager.FindByEmailAsync(newEmail);
            if (taken is not null)
            {
                await _emailService.SendAsync(newEmail, EmailTemplates.Subjects.EmailChangeUnavailable, EmailTemplates.EmailChangeUnavailable(), cancellationToken);
                return Result.Success;
            }

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            await _codeStore.StoreAsync(purpose, newEmail, code, cancellationToken);
            await _emailService.SendAsync(newEmail, EmailTemplates.Subjects.EmailChangeCode, EmailTemplates.EmailChangeCode(code), cancellationToken);
        }
        catch
        {
            await _codeStore.RemoveAsync(purpose, newEmail, cancellationToken);
            return Error.Failure("Auth.EmailFailed", "Não foi possível enviar o e-mail de verificação.");
        }

        return Result.Success;
    }
}
