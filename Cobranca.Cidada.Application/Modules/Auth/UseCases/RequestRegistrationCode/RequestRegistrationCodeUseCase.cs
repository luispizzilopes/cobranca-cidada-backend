using System.Security.Cryptography;
using ErrorOr;
using Cobranca.Cidada.Application.Common.Emails;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.AspNetCore.Identity;

namespace Cobranca.Cidada.Application.Modules.Auth.UseCases.RequestRegistrationCode;

public class RequestRegistrationCodeUseCase : IUseCase
{
    public const string Purpose = "registration";

    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IVerificationCodeStore _codeStore;

    public RequestRegistrationCodeUseCase(
        UserManager<AppUser> userManager,
        IEmailService emailService,
        IVerificationCodeStore codeStore)
    {
        _userManager = userManager;
        _emailService = emailService;
        _codeStore = codeStore;
    }

    public async Task<ErrorOr<Success>> ExecuteAsync(RequestRegistrationCodeRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();

        if (!await _codeStore.TryReserveSendAsync(Purpose, email, cancellationToken))
            return Error.Custom(429, "Auth.TooManyRequests", "Aguarde um minuto antes de pedir um novo código.");

        var existing = await _userManager.FindByEmailAsync(email);
        try
        {
            if (existing is not null)
            {
                await _emailService.SendAsync(email, EmailTemplates.Subjects.AccountAlreadyExists, EmailTemplates.AccountAlreadyExists(), cancellationToken);
                return Result.Success;
            }

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            await _codeStore.StoreAsync(Purpose, email, code, cancellationToken);
            await _emailService.SendAsync(email, EmailTemplates.Subjects.RegistrationCode, EmailTemplates.RegistrationCode(code), cancellationToken);
        }
        catch
        {
            await _codeStore.RemoveAsync(Purpose, email, cancellationToken);
            return Error.Failure("Auth.EmailFailed", "Não foi possível enviar o e-mail de verificação.");
        }

        return Result.Success;
    }
}
