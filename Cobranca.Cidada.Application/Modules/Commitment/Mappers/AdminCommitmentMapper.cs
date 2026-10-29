using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Commitment.Enums;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.AspNetCore.Identity;
using CommitmentAggregate = Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment;

namespace Cobranca.Cidada.Application.Modules.Commitment.Mappers;

public static class AdminCommitmentMapper
{
    public static AdminCommitmentSummaryResponse ToAdminSummary(this CommitmentAggregate c) => new(
        c.Slug,
        c.Title,
        new LookupResponse(c.Area.Code, c.Area.Name),
        new LookupResponse(c.Origin.Code, c.Origin.Name),
        c.CurrentStatus.ToResponse(),
        c.CurrentStatusDate,
        c.DeadlineText,
        c.DeadlineDate,
        c.IsPublished,
        c.Version,
        c.UpdatedAtUtc);

    public static AdminCommitmentDetailResponse ToAdminDetail(this CommitmentAggregate c, Func<string, string> author) => new(
        c.Slug,
        c.ExternalId,
        c.Title,
        c.Description,
        c.ShortDescription,
        c.HowItWillBeDone,
        c.HasPlanDetail,
        new LookupResponse(c.Area.Code, c.Area.Name),
        new LookupResponse(c.Origin.Code, c.Origin.Name),
        c.Date,
        c.DateKind,
        c.DeadlineText,
        c.DeadlineDate,
        c.GoalOrCondition,
        c.Classification,
        c.Evidence,
        c.PlanPage,
        c.Sources.OrderBy(s => s.Position)
            .Select(s => new SourceResponse(s.Name, s.Url, s.Page, s.Kind == SourceKind.Primary ? "primary" : "press"))
            .ToList(),
        c.CurrentStatus.ToResponse(),
        c.CurrentStatusDate,
        c.StatusEntries
            .OrderByDescending(e => e.ReferenceDate).ThenByDescending(e => e.CreatedAtUtc)
            .Select(e => new AdminStatusEntryResponse(e.Status.ToResponse(), e.ReferenceDate, e.SourceUrl, e.Note, e.IsFormalAct, author(e.AuthorId), e.CreatedAtUtc))
            .ToList(),
        c.Revisions
            .OrderByDescending(r => r.Version)
            .Select(r => new RevisionResponse(r.Version, r.Reason, author(r.AuthorId), r.CreatedAtUtc))
            .ToList(),
        c.IsPublished,
        c.PublishedAtUtc,
        c.Version,
        c.UpdatedAtUtc);
}
