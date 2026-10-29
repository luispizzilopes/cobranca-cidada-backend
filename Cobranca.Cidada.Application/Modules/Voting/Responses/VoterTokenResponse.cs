using Cobranca.Cidada.Application.Modules.Commitment.Responses;

namespace Cobranca.Cidada.Application.Modules.Voting.Responses;

public record VoterTokenResponse(string Token, DateTime DateExpiration);
