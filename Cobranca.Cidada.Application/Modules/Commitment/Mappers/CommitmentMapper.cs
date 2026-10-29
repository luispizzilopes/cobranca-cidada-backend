using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Domain.Modules.Commitment.Constants;
using Cobranca.Cidada.Domain.Modules.Commitment.Enums;
using CommitmentAggregate = Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment;

namespace Cobranca.Cidada.Application.Modules.Commitment.Mappers;

public static class CommitmentMapper
{
    public static StatusResponse ToResponse(this CommitmentStatus status) => new(status.ToString(), status.ToLabel());

    public static CommitmentSummaryResponse ToSummary(this CommitmentAggregate c, VoteSummaryResponse votes, MyVoteResponse? myVote) => new(
        c.Slug,
        c.Title,
        c.ShortDescription,
        new LookupResponse(c.Area.Code, c.Area.Name),
        new LookupResponse(c.Origin.Code, c.Origin.Name),
        c.CurrentStatus.ToResponse(),
        c.CurrentStatusDate,
        c.Date,
        c.DateKind,
        c.DeadlineText,
        c.GoalOrCondition,
        c.Classification,
        c.Evidence,
        c.PlanPage,
        c.HasPlanDetail,
        votes,
        myVote);

    public static CommitmentDetailResponse ToDetail(this CommitmentAggregate c, VoteSummaryResponse votes, MyVoteResponse? myVote) => new(
        c.Slug,
        c.ExternalId,
        c.Title,
        c.Description,
        c.ShortDescription,
        c.HowItWillBeDone,
        new LookupResponse(c.Area.Code, c.Area.Name),
        new LookupResponse(c.Origin.Code, c.Origin.Name),
        c.CurrentStatus.ToResponse(),
        c.CurrentStatusDate,
        c.Date,
        c.DateKind,
        c.DeadlineText,
        c.DeadlineDate,
        c.GoalOrCondition,
        c.Classification,
        c.Evidence,
        c.PlanPage,
        c.HasPlanDetail,
        c.Sources.OrderBy(s => s.Position)
            .Select(s => new SourceResponse(s.Name, s.Url, s.Page, s.Kind == SourceKind.Primary ? "primary" : "press"))
            .ToList(),
        c.StatusEntries
            .OrderByDescending(e => e.ReferenceDate).ThenByDescending(e => e.CreatedAtUtc)
            .Select(e => new StatusEntryResponse(e.Status.ToResponse(), e.ReferenceDate, e.SourceUrl, e.Note, e.CreatedAtUtc))
            .ToList(),
        c.Version,
        c.UpdatedAtUtc,
        votes,
        myVote);
}
