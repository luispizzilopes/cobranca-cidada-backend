namespace Cobranca.Cidada.Domain.Modules.Audit;

public static class AuditActions
{
    public const string CommitmentCreated = "commitment.created";
    public const string CommitmentUpdated = "commitment.updated";
    public const string CommitmentUnpublished = "commitment.unpublished";
    public const string CommitmentPublished = "commitment.published";
    public const string StatusRegistered = "commitment.status_registered";
    public const string VoteBurstBlocked = "vote.burst_blocked";
    public const string VoteBurstOnCommitment = "vote.burst_on_commitment";
    public const string SeedOverwrite = "commitment.seed_overwritten";
    public const string AreaRenamed = "catalog.area_renamed";
    public const string OriginRenamed = "catalog.origin_renamed";
    public const string CommentRemoved = "comment.removed";
}
