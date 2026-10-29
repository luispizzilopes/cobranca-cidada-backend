namespace Cobranca.Cidada.Application.Modules.Voting.Requests;

public record VerifyVoterCodeRequest(string Email, string Code);
