using Cobranca.Cidada.Application.Modules.Commitment.Responses;

namespace Cobranca.Cidada.Application.Modules.Voting.Responses;

public record MyVoteItemResponse(string CommitmentId, string Title, string? Rating, bool Charge, DateTime UpdatedAtUtc);
