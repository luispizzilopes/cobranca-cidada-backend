using ErrorOr;
using Cobranca.Cidada.Application.Common.Emails;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using Cobranca.Cidada.Application.Modules.Auth.UseCases.RequestPasswordReset;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.AspNetCore.Identity;

namespace Cobranca.Cidada.Application.Modules.Auth.UseCases.ConfirmPasswordReset;

public class ConfirmPasswordResetUseCase : IUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IVerificationCodeStore _codeStore;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public ConfirmPasswordResetUseCase(
        UserManager<AppUser> userManager,
        IVerificationCodeStore codeStore,
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _userManager = userManager;
        _codeStore = codeStore;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<ErrorOr<Success>> ExecuteAsync(ConfirmPasswordResetRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();

        var outcome = await _codeStore.VerifyAsync(RequestPasswordResetUseCase.Purpose, email, request.Code.Trim(), cancellationToken);
        if (outcome != VerificationOutcome.Valid)
            return Register.RegisterUseCase.CodeError(outcome);

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || !user.Active)
            return Error.Validation("Auth.InvalidCode", "Código inválido.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        if (!result.Succeeded)
            return result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();

        user.LockoutLevel = 0;
        await _userManager.ResetAccessFailedCountAsync(user);
        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.UpdateAsync(user);

        await _unitOfWork.UserSessionRepository.RevokeAllAsync(user.Id, "Senha redefinida.", null, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        try
        {
            await _emailService.SendAsync(user.Email!, EmailTemplates.Subjects.PasswordChanged, EmailTemplates.PasswordChanged(), cancellationToken);
        }
        catch
        {
        }

        return Result.Success;
    }
}
