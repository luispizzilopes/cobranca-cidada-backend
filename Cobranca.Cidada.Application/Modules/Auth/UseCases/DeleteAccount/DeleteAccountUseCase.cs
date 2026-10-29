using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using Cobranca.Cidada.Application.Modules.Auth.Services;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.AspNetCore.Identity;

namespace Cobranca.Cidada.Application.Modules.Auth.UseCases.DeleteAccount;

/// <summary>Exclusão de conta em autoatendimento (LGPD): apaga usuário, sessões e todos os votos e comentários ligados ao e-mail.</summary>
public class DeleteAccountUseCase : IUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVoterKeyService _voterKeyService;
    private readonly AccountReauthenticationService _reauthentication;

    public DeleteAccountUseCase(
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        IVoterKeyService voterKeyService,
        AccountReauthenticationService reauthentication)
    {
        _reauthentication = reauthentication;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _voterKeyService = voterKeyService;
    }

    public async Task<ErrorOr<Success>> ExecuteAsync(string userId, DeleteAccountRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Error.NotFound("Auth.UserNotFound", "Usuário não encontrado.");

        var confirmed = await _reauthentication.ConfirmAsync(user, request.Password, request.GoogleCredential, cancellationToken);
        if (confirmed.IsError)
            return confirmed.Errors;

        var voterKey = _voterKeyService.Compute(user.Email!);
        await _unitOfWork.VoteRepository.DeleteAllByVoterAsync(voterKey, cancellationToken);
        await _unitOfWork.CommentRepository.DeleteAllByVoterAsync(voterKey, cancellationToken);
        await _unitOfWork.VoterIdentityRepository.DeleteAsync(voterKey, cancellationToken);
        await _unitOfWork.UserSessionRepository.RevokeAllAsync(user.Id, "Conta excluída pelo usuário.", null, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded
            ? Result.Success
            : Error.Failure("Auth.DeleteFailed", "Não foi possível excluir a conta.");
    }
}
