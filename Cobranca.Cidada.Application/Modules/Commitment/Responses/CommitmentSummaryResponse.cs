namespace Cobranca.Cidada.Application.Modules.Commitment.Responses;

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
