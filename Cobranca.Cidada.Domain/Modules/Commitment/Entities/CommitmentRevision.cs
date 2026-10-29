using Cobranca.Cidada.BuildingBlocks.Common.Class.Base;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;

namespace Cobranca.Cidada.Domain.Modules.Commitment.Entities;

/// <summary>Conteúdo anterior de um compromisso, guardado a cada edição.</summary>
public class CommitmentRevision : Entity<CommitmentRevisionId>
{
    public int Version { get; private set; }
    public string SnapshotJson { get; private set; } = null!;
    public string AuthorId { get; private set; } = null!;
    public string Reason { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }

    private CommitmentRevision() { }

    internal CommitmentRevision(int version, string snapshotJson, string authorId, string reason)
        : base(CommitmentRevisionId.CreateIdentifier())
    {
        Version = version;
        SnapshotJson = snapshotJson;
        AuthorId = authorId;
        Reason = reason;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
