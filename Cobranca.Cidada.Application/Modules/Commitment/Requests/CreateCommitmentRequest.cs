namespace Cobranca.Cidada.Application.Modules.Commitment.Requests;

/// <summary>
/// Novo compromisso. <paramref name="Slug"/> é o identificador público (ex.: ECON-050); vazio gera o próximo da área.
/// Nasce despublicado, a menos que <paramref name="Publish"/> seja verdadeiro. A situação inicial é "Não avaliado".
/// </summary>
public record CreateCommitmentRequest(
    string? Slug,
    string AreaCode,
    string OriginCode,
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
    IReadOnlyList<string>? Classification,
    IReadOnlyList<string>? Evidence,
    string? PlanPage,
    IReadOnlyList<SourceInput>? Sources,
    bool Publish);
