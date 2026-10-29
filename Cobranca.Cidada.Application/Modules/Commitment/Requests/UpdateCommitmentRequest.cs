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
