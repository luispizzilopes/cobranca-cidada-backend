namespace Cobranca.Cidada.Application.Modules.Commitment.Responses;

public record AdminCommitmentSummaryResponse(
    string Id,
    string Title,
    LookupResponse Area,
    LookupResponse Origin,
    StatusResponse Status,
    DateOnly? StatusDate,
    string? DeadlineText,
    DateOnly? DeadlineDate,
    bool IsPublished,
    int Version,
    DateTime UpdatedAtUtc);
