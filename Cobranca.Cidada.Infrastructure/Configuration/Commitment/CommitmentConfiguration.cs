using Cobranca.Cidada.Domain.Modules.Catalog.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Commitment.Entities;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Mandate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CommitmentAggregate = Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment;
using MandateAggregate = Cobranca.Cidada.Domain.Modules.Mandate.Aggregates.Mandate;

namespace Cobranca.Cidada.Infrastructure.Configuration.Commitment;

public class CommitmentConfiguration : IEntityTypeConfiguration<CommitmentAggregate>
{
    public void Configure(EntityTypeBuilder<CommitmentAggregate> builder)
    {
        builder.ToTable("Commitments");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => CommitmentId.Create(value))
            .IsRequired();

        builder.Property(c => c.ExternalId).HasMaxLength(100);
        builder.Property(c => c.Slug).HasMaxLength(120).IsRequired();

        builder.Property(c => c.MandateId)
            .HasConversion(id => id.Value, value => MandateId.Create(value))
            .IsRequired();
        builder.Property(c => c.AreaId)
            .HasConversion(id => id.Value, value => AreaId.Create(value))
            .IsRequired();
        builder.Property(c => c.OriginId)
            .HasConversion(id => id.Value, value => OriginId.Create(value))
            .IsRequired();

        builder.Property(c => c.Title).HasMaxLength(300).IsRequired();
        builder.Property(c => c.Description).IsRequired();
        builder.Property(c => c.ShortDescription);
        builder.Property(c => c.HowItWillBeDone);
        builder.Property(c => c.HasPlanDetail).IsRequired();
        builder.Property(c => c.DateKind).HasMaxLength(100);
        builder.Property(c => c.DeadlineText).HasMaxLength(300);
        builder.Property(c => c.GoalOrCondition);
        builder.Property(c => c.Classification).HasColumnType("text[]").IsRequired();
        builder.Property(c => c.Evidence).HasColumnType("text[]").IsRequired();
        builder.Property(c => c.PlanPage).HasMaxLength(50);

        builder.Property(c => c.CurrentStatus).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(c => c.CurrentStatusDate);

        builder.Property(c => c.IsPublished).IsRequired();
        builder.Property(c => c.Version).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(450).IsRequired();

        builder.Property(c => c.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(c => c.UpdatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

        builder.HasOne(c => c.Area).WithMany().HasForeignKey(c => c.AreaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Origin).WithMany().HasForeignKey(c => c.OriginId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MandateAggregate>().WithMany().HasForeignKey(c => c.MandateId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Sources).WithOne().HasForeignKey("CommitmentId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.StatusEntries).WithOne().HasForeignKey("CommitmentId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Revisions).WithOne().HasForeignKey("CommitmentId").OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Sources).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(c => c.StatusEntries).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(c => c.Revisions).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => c.ExternalId).IsUnique();
        builder.HasIndex(c => new { c.IsPublished, c.AreaId });
        builder.HasIndex(c => new { c.IsPublished, c.CurrentStatus });

        builder.Property<uint>("xmin").IsRowVersion();
    }
}
