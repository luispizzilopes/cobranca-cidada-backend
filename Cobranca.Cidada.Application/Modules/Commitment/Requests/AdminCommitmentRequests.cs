namespace Cobranca.Cidada.Application.Modules.Commitment.Requests;

/// <summary>
/// Edição de um compromisso pelo painel. O identificador público, o ID externo e o mandato não mudam.
/// <paramref name="ExpectedVersion"/> é a versão que a pessoa estava vendo: se outra edição passou antes, a requisição é recusada.
/// </summary>
public record UpdateCommitmentRequest(
    int ExpectedVersion,
    string Reason,
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
    IReadOnlyList<SourceInput>? Sources);

/// <summary>Novo registro no histórico de situação. <paramref name="Status"/> aceita o rótulo em português ou o nome do enum.</summary>
public record RegisterStatusRequest(
    string Status,
    DateOnly ReferenceDate,
    string? SourceUrl,
    string? Note,
    bool IsFormalAct);

public record SetPublicationRequest(bool Published);

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
