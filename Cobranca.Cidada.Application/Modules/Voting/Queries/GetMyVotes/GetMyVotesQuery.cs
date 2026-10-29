using ErrorOr;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Voting.Queries.GetMyVotes;

public record GetMyVotesQuery(string VoterKey) : IRequest<ErrorOr<IReadOnlyList<MyVoteItemResponse>>>;
