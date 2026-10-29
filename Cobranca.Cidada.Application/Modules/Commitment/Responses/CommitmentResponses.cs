namespace Cobranca.Cidada.Application.Modules.Commitment.Responses;

public record LookupResponse(string Code, string Name);

public record StatusResponse(string Value, string Label);

public record SourceResponse(string Name, string Url, string? Page, string Kind);

public record StatusEntryResponse(StatusResponse Status, DateOnly ReferenceDate, string? SourceUrl, string? Note, DateTime RecordedAtUtc);

public record CommitmentSummaryResponse(
    string Id,
    string Title,
    string? ShortDescription,
    LookupResponse Area,
    LookupResponse Origin,
    StatusResponse Status,
    DateOnly? StatusDate,
    DateOnly? Date,
    string? DateKind,
    string? DeadlineText,
    string? GoalOrCondition,
    IReadOnlyList<string> Classification,
    IReadOnlyList<string> Evidence,
    string? PlanPage,
    bool HasPlanDetail,
    VoteSummaryResponse Votes,
    MyVoteResponse? MyVote);

public record CommitmentDetailResponse(
    string Id,
    string? ExternalId,
    string Title,
    string Description,
    string? ShortDescription,
    string? HowItWillBeDone,
    LookupResponse Area,
    LookupResponse Origin,
    StatusResponse Status,
    DateOnly? StatusDate,
    DateOnly? Date,
    string? DateKind,
    string? DeadlineText,
    DateOnly? DeadlineDate,
    string? GoalOrCondition,
    IReadOnlyList<string> Classification,
    IReadOnlyList<string> Evidence,
    string? PlanPage,
    bool HasPlanDetail,
    IReadOnlyList<SourceResponse> Sources,
    IReadOnlyList<StatusEntryResponse> StatusHistory,
    int Version,
    DateTime UpdatedAtUtc,
    VoteSummaryResponse Votes,
    MyVoteResponse? MyVote);
