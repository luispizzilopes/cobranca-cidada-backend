using Cobranca.Cidada.Domain.Modules.Commitment.Entities;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobranca.Cidada.Infrastructure.Configuration.Commitment;

public class CommitmentRevisionConfiguration : IEntityTypeConfiguration<CommitmentRevision>
{
    public void Configure(EntityTypeBuilder<CommitmentRevision> builder)
    {
        builder.ToTable("CommitmentRevisions");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => CommitmentRevisionId.Create(value))
            .IsRequired();

        builder.Property<CommitmentId>("CommitmentId")
            .HasConversion(id => id.Value, value => CommitmentId.Create(value))
            .IsRequired();

        builder.Property(r => r.Version).IsRequired();
        builder.Property(r => r.SnapshotJson).HasColumnType("jsonb").IsRequired();
        builder.Property(r => r.AuthorId).HasMaxLength(450).IsRequired();
        builder.Property(r => r.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(r => r.CreatedAtUtc).IsRequired();

        builder.HasIndex("CommitmentId", nameof(CommitmentRevision.Version)).IsUnique();
    }
}
