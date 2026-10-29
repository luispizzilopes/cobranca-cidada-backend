namespace Cobranca.Cidada.Application.Modules.Commitment.Requests;

/// <summary>Novo registro no histórico de situação. <paramref name="Status"/> aceita o rótulo em português ou o nome do enum.</summary>
public record RegisterStatusRequest(
    string Status,
    DateOnly ReferenceDate,
    string? SourceUrl,
    string? Note,
    bool IsFormalAct);
