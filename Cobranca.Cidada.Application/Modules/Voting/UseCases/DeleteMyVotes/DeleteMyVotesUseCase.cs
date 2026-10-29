using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;

namespace Cobranca.Cidada.Application.Modules.Voting.UseCases.DeleteMyVotes;

public class DeleteMyVotesUseCase : IUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMyVotesUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<DeleteVotesResponse>> ExecuteAsync(string voterKey, bool forgetVoterIdentity, CancellationToken cancellationToken = default)
    {
        var deleted = await _unitOfWork.VoteRepository.DeleteAllByVoterAsync(voterKey, cancellationToken);
        if (forgetVoterIdentity)
            await _unitOfWork.VoterIdentityRepository.DeleteAsync(voterKey, cancellationToken);

        return new DeleteVotesResponse(deleted);
    }
}
