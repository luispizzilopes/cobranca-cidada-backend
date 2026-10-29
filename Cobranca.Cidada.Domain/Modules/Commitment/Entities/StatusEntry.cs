using Cobranca.Cidada.BuildingBlocks.Common.Class.Base;
using Cobranca.Cidada.Domain.Modules.Commitment.Enums;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;

namespace Cobranca.Cidada.Domain.Modules.Commitment.Entities;

/// <summary>Registro imutável do histórico de situação de execução.</summary>
public class StatusEntry : Entity<StatusEntryId>
{
    public CommitmentStatus Status { get; private set; }
    public DateOnly ReferenceDate { get; private set; }
    public string? SourceUrl { get; private set; }
    public string? Note { get; private set; }
    public bool IsFormalAct { get; private set; }
    public string AuthorId { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }

    private StatusEntry() { }

    internal StatusEntry(
        CommitmentStatus status,
        DateOnly referenceDate,
        string? sourceUrl,
        string? note,
        bool isFormalAct,
        string authorId)
        : base(StatusEntryId.CreateIdentifier())
    {
        Status = status;
        ReferenceDate = referenceDate;
        SourceUrl = sourceUrl;
        Note = note;
        IsFormalAct = isFormalAct;
        AuthorId = authorId;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
