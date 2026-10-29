using Cobranca.Cidada.Domain.Modules.Comment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CommentAggregate = Cobranca.Cidada.Domain.Modules.Comment.Aggregates.Comment;
using CommitmentAggregate = Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment;

namespace Cobranca.Cidada.Infrastructure.Configuration.Comment;

public class CommentConfiguration : IEntityTypeConfiguration<CommentAggregate>
{
    public void Configure(EntityTypeBuilder<CommentAggregate> builder)
    {
        builder.ToTable("Comments");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => CommentId.Create(value))
            .IsRequired();

        builder.Property(c => c.VoterKey).HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(c => c.CommitmentId)
            .HasConversion(id => id.Value, value => CommitmentId.Create(value))
            .IsRequired();
        builder.Property(c => c.ParentId).HasConversion(id => id!.Value, value => CommentId.Create(value));
        builder.Property(c => c.Text).HasMaxLength(CommentAggregate.MaxLength).IsRequired();
        builder.Property(c => c.CreatedAtUtc).IsRequired();

        builder.HasOne<CommitmentAggregate>().WithMany().HasForeignKey(c => c.CommitmentId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<CommentAggregate>().WithMany().HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.ParentId);
        builder.HasIndex(c => new { c.CommitmentId, c.CreatedAtUtc });
        builder.HasIndex(c => c.VoterKey);
    }
}
