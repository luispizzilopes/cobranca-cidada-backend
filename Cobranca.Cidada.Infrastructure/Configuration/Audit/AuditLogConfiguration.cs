using Cobranca.Cidada.Domain.Modules.Audit.Entities;
using Cobranca.Cidada.Domain.Modules.Audit.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobranca.Cidada.Infrastructure.Configuration.Audit;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => AuditLogId.Create(value))
            .IsRequired();

        builder.Property(a => a.Actor).HasMaxLength(450).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Details).HasMaxLength(4000);
        builder.Property(a => a.OccurredAtUtc).IsRequired();

        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => a.OccurredAtUtc);
    }
}
