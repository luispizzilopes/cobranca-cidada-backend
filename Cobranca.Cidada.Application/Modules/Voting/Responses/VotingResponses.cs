using Cobranca.Cidada.Application.Modules.Commitment.Responses;

namespace Cobranca.Cidada.Application.Modules.Voting.Responses;

public record VoterTokenResponse(string Token, DateTime DateExpiration);

public record VoteStateResponse(string CommitmentId, MyVoteResponse? MyVote, VoteSummaryResponse Votes);

public record MyVoteItemResponse(string CommitmentId, string Title, string? Rating, bool Charge, DateTime UpdatedAtUtc);

public record DeleteVotesResponse(int DeletedVotes);
