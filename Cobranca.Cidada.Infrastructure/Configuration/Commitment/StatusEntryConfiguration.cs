using Cobranca.Cidada.Domain.Modules.Commitment.Entities;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobranca.Cidada.Infrastructure.Configuration.Commitment;

public class StatusEntryConfiguration : IEntityTypeConfiguration<StatusEntry>
{
    public void Configure(EntityTypeBuilder<StatusEntry> builder)
    {
        builder.ToTable("StatusEntries");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => StatusEntryId.Create(value))
            .IsRequired();

        builder.Property<CommitmentId>("CommitmentId")
            .HasConversion(id => id.Value, value => CommitmentId.Create(value))
            .IsRequired();

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(e => e.ReferenceDate).IsRequired();
        builder.Property(e => e.SourceUrl).HasMaxLength(2000);
        builder.Property(e => e.Note).HasMaxLength(2000);
        builder.Property(e => e.IsFormalAct).IsRequired();
        builder.Property(e => e.AuthorId).HasMaxLength(450).IsRequired();
        builder.Property(e => e.CreatedAtUtc).IsRequired();

        builder.HasIndex("CommitmentId", nameof(StatusEntry.ReferenceDate));
    }
}
