using Cobranca.Cidada.Application.Modules.Commitment.Responses;

namespace Cobranca.Cidada.Application.Modules.Voting.Responses;

public record VoteStateResponse(string CommitmentId, MyVoteResponse? MyVote, VoteSummaryResponse Votes);
