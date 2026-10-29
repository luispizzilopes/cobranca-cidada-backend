using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Voting.Aggregates;
using Cobranca.Cidada.Domain.Modules.Voting.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CommitmentAggregate = Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment;

namespace Cobranca.Cidada.Infrastructure.Configuration.Voting;

public class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        builder.ToTable("Votes");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .HasConversion(id => id.Value, value => VoteId.Create(value))
            .IsRequired();

        builder.Property(v => v.VoterKey).HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(v => v.CommitmentId)
            .HasConversion(id => id.Value, value => CommitmentId.Create(value))
            .IsRequired();
        builder.Property(v => v.Rating).HasConversion<string>().HasMaxLength(10);
        builder.Property(v => v.Charge).IsRequired();
        builder.Property(v => v.UpdatedAtUtc).IsRequired();

        builder.HasOne<CommitmentAggregate>().WithMany().HasForeignKey(v => v.CommitmentId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => new { v.VoterKey, v.CommitmentId }).IsUnique();
        builder.HasIndex(v => v.CommitmentId);

        builder.Ignore(v => v.IsEmpty);
    }
}
