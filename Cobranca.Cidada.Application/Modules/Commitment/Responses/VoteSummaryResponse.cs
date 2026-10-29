namespace Cobranca.Cidada.Application.Modules.Commitment.Responses;

/// <summary>
/// Contagens públicas de opinião, sempre informadas. Os campos "Hidden" são mantidos por compatibilidade
/// e vêm sempre falsos.
/// </summary>
public record VoteSummaryResponse(
    int? Charges,
    bool ChargesHidden,
    int? Good,
    int? Bad,
    bool RatingsHidden);
