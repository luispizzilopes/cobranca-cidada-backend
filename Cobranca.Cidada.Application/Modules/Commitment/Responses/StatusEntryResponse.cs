namespace Cobranca.Cidada.Application.Modules.Commitment.Responses;

public record StatusEntryResponse(StatusResponse Status, DateOnly ReferenceDate, string? SourceUrl, string? Note, DateTime RecordedAtUtc);
