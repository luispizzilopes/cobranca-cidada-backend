namespace Cobranca.Cidada.Application.Modules.Commitment.Requests;

public record SourceInput(string Name, string Url, string? Page, string Kind);

public record InitialStatusInput(string Status, DateOnly ReferenceDate, string? SourceUrl, string? Note, bool IsFormalAct);

/// <summary>
/// Forma única de entrada de um compromisso. Usada pelo seed e pelo painel de curadoria,
/// para que a validação seja a mesma nos dois caminhos.
/// </summary>
public record CommitmentInput(
    string? ExternalId,
    string? Slug,
    string AreaCode,
    string AreaName,
    string OriginName,
    string Title,
    string Description,
    string? ShortDescription,
    string? HowItWillBeDone,
    bool HasPlanDetail,
    DateOnly? Date,
    string? DateKind,
    string? DeadlineText,
    DateOnly? DeadlineDate,
    string? GoalOrCondition,
    IReadOnlyList<string> Classification,
    IReadOnlyList<string> Evidence,
    string? PlanPage,
    IReadOnlyList<SourceInput> Sources,
    InitialStatusInput? InitialStatus);
