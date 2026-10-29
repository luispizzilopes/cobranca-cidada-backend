using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Application.Modules.Voting.Policies;

namespace Cobranca.Cidada.Application.Modules.Voting.UseCases.GetMyVotes;

/// <summary>Os votos da própria pessoa. Serve também como exportação dos dados pessoais (LGPD).</summary>
public class GetMyVotesUseCase : IUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyVotesUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<IReadOnlyList<MyVoteItemResponse>>> ExecuteAsync(string voterKey, CancellationToken cancellationToken = default)
    {
        var votes = await _unitOfWork.VoteRepository.GetAllByVoterAsync(voterKey, cancellationToken);
        var headers = await _unitOfWork.CommitmentRepository.GetHeadersByIdsAsync(votes.Select(v => v.CommitmentId.Value), cancellationToken);

        IReadOnlyList<MyVoteItemResponse> items = votes
            .Where(v => headers.ContainsKey(v.CommitmentId.Value))
            .OrderByDescending(v => v.UpdatedAtUtc)
            .Select(v => new MyVoteItemResponse(
                headers[v.CommitmentId.Value].Slug,
                headers[v.CommitmentId.Value].Title,
                VotePolicy.RatingToText(v.Rating),
                v.Charge,
                v.UpdatedAtUtc))
            .ToList();

        return ErrorOr.ErrorOrFactory.From(items);
    }
}
