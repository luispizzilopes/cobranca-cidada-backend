using ErrorOr;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using Cobranca.Cidada.Application.Modules.Voting.UseCases.GetMyVotes;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Voting.Queries.GetMyVotes;

public class GetMyVotesQueryHandler : IRequestHandler<GetMyVotesQuery, ErrorOr<IReadOnlyList<MyVoteItemResponse>>>
{
    private readonly GetMyVotesUseCase _useCase;

    public GetMyVotesQueryHandler(GetMyVotesUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<IReadOnlyList<MyVoteItemResponse>>> Handle(GetMyVotesQuery query, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(query.VoterKey, cancellationToken);
}
