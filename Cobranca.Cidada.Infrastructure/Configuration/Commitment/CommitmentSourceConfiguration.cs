using Cobranca.Cidada.Domain.Modules.Commitment.Entities;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobranca.Cidada.Infrastructure.Configuration.Commitment;

public class CommitmentSourceConfiguration : IEntityTypeConfiguration<CommitmentSource>
{
    public void Configure(EntityTypeBuilder<CommitmentSource> builder)
    {
        builder.ToTable("CommitmentSources");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => CommitmentSourceId.Create(value))
            .IsRequired();

        builder.Property<CommitmentId>("CommitmentId")
            .HasConversion(id => id.Value, value => CommitmentId.Create(value))
            .IsRequired();

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Url).HasMaxLength(2000).IsRequired();
        builder.Property(s => s.Page).HasMaxLength(50);
        builder.Property(s => s.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(s => s.Position).IsRequired();

        builder.HasIndex("CommitmentId");
    }
}
