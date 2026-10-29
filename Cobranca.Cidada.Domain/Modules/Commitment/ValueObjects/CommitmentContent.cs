using Cobranca.Cidada.Domain.Modules.Commitment.Enums;

namespace Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;

public record SourceContent(string Name, string Url, string? Page, SourceKind Kind);

/// <summary>Conteúdo editável de um compromisso (tudo que gera revisão quando muda).</summary>
public record CommitmentContent(
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
    IReadOnlyList<SourceContent> Sources);
