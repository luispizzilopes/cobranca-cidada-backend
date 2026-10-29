namespace Cobranca.Cidada.Application.Modules.Commitment.Responses;

public record RevisionResponse(int Version, string Reason, string Author, DateTime CreatedAtUtc);
