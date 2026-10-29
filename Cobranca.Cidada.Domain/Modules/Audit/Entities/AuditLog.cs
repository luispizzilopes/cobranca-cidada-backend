using Cobranca.Cidada.BuildingBlocks.Common.Class.Base;
using Cobranca.Cidada.Domain.Modules.Audit.ValueObjects;

namespace Cobranca.Cidada.Domain.Modules.Audit.Entities;

public class AuditLog : Entity<AuditLogId>
{
    public string Actor { get; private set; } = null!;
    public string Action { get; private set; } = null!;
    public string EntityType { get; private set; } = null!;
    public string EntityId { get; private set; } = null!;
    public string? Details { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    private AuditLog() { }

    private AuditLog(string actor, string action, string entityType, string entityId, string? details, DateTime occurredAtUtc)
        : base(AuditLogId.CreateIdentifier())
    {
        Actor = actor;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        Details = details;
        OccurredAtUtc = occurredAtUtc;
    }

    public static AuditLog Create(string actor, string action, string entityType, string entityId, string? details = null)
        => new(actor, action, entityType, entityId, details, DateTime.UtcNow);
}
