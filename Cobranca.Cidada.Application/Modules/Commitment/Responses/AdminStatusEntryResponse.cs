namespace Cobranca.Cidada.Application.Modules.Commitment.Responses;

public record AdminStatusEntryResponse(
    StatusResponse Status,
    DateOnly ReferenceDate,
    string? SourceUrl,
    string? Note,
    bool IsFormalAct,
    string Author,
    DateTime RecordedAtUtc);
