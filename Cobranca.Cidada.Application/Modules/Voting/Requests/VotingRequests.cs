namespace Cobranca.Cidada.Application.Modules.Voting.Requests;

public record RequestVoterCodeRequest(string Email);

public record VerifyVoterCodeRequest(string Email, string Code);

/// <summary>Rating: "boa", "ma" ou nulo. Charge: marca "Cobrar". Rating nulo e Charge falso desfazem o voto.</summary>
public record SetVoteRequest(string? Rating, bool Charge);
