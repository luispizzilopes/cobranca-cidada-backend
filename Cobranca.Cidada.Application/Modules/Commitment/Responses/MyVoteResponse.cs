namespace Cobranca.Cidada.Application.Modules.Commitment.Responses;

/// <summary>Voto da própria pessoa. Visível só para ela.</summary>
public record MyVoteResponse(string? Rating, bool Charge);
