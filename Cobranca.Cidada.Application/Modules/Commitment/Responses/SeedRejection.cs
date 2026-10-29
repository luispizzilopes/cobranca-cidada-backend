namespace Cobranca.Cidada.Application.Modules.Commitment.Responses;

public record SeedRejection(string Id, IReadOnlyList<string> Reasons);
