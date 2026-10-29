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

public record AdminStatusEntryResponse(
    StatusResponse Status,
    DateOnly ReferenceDate,
    string? SourceUrl,
    string? Note,
    bool IsFormalAct,
    string Author,
    DateTime RecordedAtUtc);

public record RevisionResponse(int Version, string Reason, string Author, DateTime CreatedAtUtc);

/// <summary>Tudo que o painel precisa para revisar e editar um compromisso, publicado ou não.</summary>
public record AdminCommitmentDetailResponse(
    string Id,
    string? ExternalId,
    string Title,
    string Description,
    string? ShortDescription,
    string? HowItWillBeDone,
    bool HasPlanDetail,
    LookupResponse Area,
    LookupResponse Origin,
    DateOnly? Date,
    string? DateKind,
    string? DeadlineText,
    DateOnly? DeadlineDate,
    string? GoalOrCondition,
    IReadOnlyList<string> Classification,
    IReadOnlyList<string> Evidence,
    string? PlanPage,
    IReadOnlyList<SourceResponse> Sources,
    StatusResponse Status,
    DateOnly? StatusDate,
    IReadOnlyList<AdminStatusEntryResponse> StatusHistory,
    IReadOnlyList<RevisionResponse> Revisions,
    bool IsPublished,
    DateTime? PublishedAtUtc,
    int Version,
    DateTime UpdatedAtUtc);
