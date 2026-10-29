using ErrorOr;
using Cobranca.Cidada.Application.Common.Emails;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.AspNetCore.Identity;

namespace Cobranca.Cidada.Application.Modules.Auth.UseCases.ChangePassword;

/// <summary>Troca de senha pela própria pessoa logada: confere a senha atual e encerra todas as sessões.</summary>
public class ChangePasswordUseCase : IUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public ChangePasswordUseCase(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<ErrorOr<Success>> ExecuteAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || !user.Active)
            return Error.NotFound("Auth.UserNotFound", "Usuário não encontrado.");

        if (!await _userManager.HasPasswordAsync(user))
            return Error.Validation("Auth.NoPassword", "Sua conta entra com o Google e não tem senha. Para criar uma, use \"Esqueci minha senha\" na tela de entrada.");

        if (!await _userManager.CheckPasswordAsync(user, request.CurrentPassword))
            return Error.Unauthorized("Auth.InvalidCredentials", "Senha atual incorreta.");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();

        await _unitOfWork.UserSessionRepository.RevokeAllAsync(user.Id, "Senha alterada pelo usuário.", null, cancellationToken);
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
