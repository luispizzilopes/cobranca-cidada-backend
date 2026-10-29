using ErrorOr;
using Cobranca.Cidada.Application.Common.Emails;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using Cobranca.Cidada.Application.Modules.Auth.UseCases.RequestEmailChange;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.AspNetCore.Identity;

namespace Cobranca.Cidada.Application.Modules.Auth.UseCases.ConfirmEmailChange;

/// <summary>
/// Conclui a troca de e-mail. Como a identidade do votante é derivada do e-mail, os votos e comentários migram para a nova chave
/// na mesma transação da troca; se algo falhar, nada muda. Todas as sessões são encerradas, porque os tokens
/// emitidos carregam a chave antiga.
/// </summary>
public class ConfirmEmailChangeUseCase : IUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IVerificationCodeStore _codeStore;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVoterKeyService _voterKeyService;
    private readonly IEmailService _emailService;

    public ConfirmEmailChangeUseCase(
        UserManager<AppUser> userManager,
        IVerificationCodeStore codeStore,
        IUnitOfWork unitOfWork,
        IVoterKeyService voterKeyService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _codeStore = codeStore;
        _unitOfWork = unitOfWork;
        _voterKeyService = voterKeyService;
        _emailService = emailService;
    }

    public async Task<ErrorOr<Success>> ExecuteAsync(string userId, ConfirmEmailChangeRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Error.NotFound("Auth.UserNotFound", "Usuário não encontrado.");

        var newEmail = request.NewEmail.Trim();
        var outcome = await _codeStore.VerifyAsync(RequestEmailChangeUseCase.PurposeFor(user.Id), newEmail, request.Code.Trim(), cancellationToken);
        if (outcome != VerificationOutcome.Valid)
            return Register.RegisterUseCase.CodeError(outcome);

        var taken = await _userManager.FindByEmailAsync(newEmail);
        if (taken is not null && taken.Id != user.Id)
            return Error.Validation("Auth.EmailChangeFailed", "Não foi possível alterar o e-mail.");

        var oldEmail = user.Email!;
        var oldKey = _voterKeyService.Compute(oldEmail);
        var newKey = _voterKeyService.Compute(newEmail);

        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var setEmail = await _userManager.SetEmailAsync(user, newEmail);
                var setUserName = setEmail.Succeeded ? await _userManager.SetUserNameAsync(user, newEmail) : setEmail;
                if (!setUserName.Succeeded)
                    throw new EmailChangeRejectedException();

                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                await _unitOfWork.VoteRepository.MoveAsync(oldKey, newKey, ct);
                await _unitOfWork.CommentRepository.MoveAsync(oldKey, newKey, ct);
                await _unitOfWork.VoterIdentityRepository.DeleteAsync(oldKey, ct);
                await _unitOfWork.UserSessionRepository.RevokeAllAsync(user.Id, "E-mail da conta alterado.", null, ct);
                await _unitOfWork.CommitAsync(ct);
            }, cancellationToken);
        }
        catch (EmailChangeRejectedException)
        {
            return Error.Validation("Auth.EmailChangeFailed", "Não foi possível alterar o e-mail.");
        }

        try
        {
            await _emailService.SendAsync(oldEmail, EmailTemplates.Subjects.EmailChanged, EmailTemplates.EmailChanged(newEmail), cancellationToken);
        }
        catch
        {
        }

        return Result.Success;
    }

    private sealed class EmailChangeRejectedException : Exception;
}
