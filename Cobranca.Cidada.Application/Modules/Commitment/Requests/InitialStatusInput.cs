namespace Cobranca.Cidada.Application.Modules.Commitment.Requests;

public record InitialStatusInput(string Status, DateOnly ReferenceDate, string? SourceUrl, string? Note, bool IsFormalAct);
