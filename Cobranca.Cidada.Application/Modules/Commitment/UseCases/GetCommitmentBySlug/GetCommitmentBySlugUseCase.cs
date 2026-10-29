using ErrorOr;
using Cobranca.Cidada.Application.Common;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;

namespace Cobranca.Cidada.Application.Modules.Commitment.UseCases.GetCommitmentBySlug;

public class GetCommitmentBySlugUseCase : IUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly VotePolicy _votePolicy;

    public GetCommitmentBySlugUseCase(IUnitOfWork unitOfWork, VotePolicy votePolicy)
    {
        _unitOfWork = unitOfWork;
        _votePolicy = votePolicy;
    }

    public async Task<ErrorOr<CommitmentDetailResponse>> ExecuteAsync(string slug, string? voterKey, CancellationToken cancellationToken = default)
    {
        var commitment = await _unitOfWork.CommitmentRepository.GetBySlugAsync(slug.Trim(), onlyPublished: true, cancellationToken);
        if (commitment is null)
            return Error.NotFound("Commitment.NotFound", "Compromisso não encontrado.");

        var id = commitment.Id.Value;
        var tally = (await _unitOfWork.VoteRepository.GetTalliesAsync([id], cancellationToken)).FirstOrDefault();
        var mine = voterKey is null
            ? null
            : (await _unitOfWork.VoteRepository.GetViewsAsync(voterKey, [id], cancellationToken)).FirstOrDefault();

        return commitment.ToDetail(_votePolicy.Summarize(tally), VotePolicy.ToMyVote(mine));
    }
}
