using Cobranca.Cidada.Domain.Modules.Mandate.Entities;
using Cobranca.Cidada.Domain.Modules.Mandate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MandateAggregate = Cobranca.Cidada.Domain.Modules.Mandate.Aggregates.Mandate;

namespace Cobranca.Cidada.Infrastructure.Configuration.Mandate;

public class MandateConfiguration : IEntityTypeConfiguration<MandateAggregate>
{
    public void Configure(EntityTypeBuilder<MandateAggregate> builder)
    {
        builder.ToTable("Mandates");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasConversion(id => id.Value, value => MandateId.Create(value))
            .IsRequired();

        builder.Property(m => m.OfficialId)
            .HasConversion(id => id.Value, value => OfficialId.Create(value))
            .IsRequired();

        builder.Property(m => m.Title).HasMaxLength(200).IsRequired();
        builder.Property(m => m.StartDate).IsRequired();
        builder.Property(m => m.EndDate).IsRequired();

        builder.HasOne<Official>().WithMany().HasForeignKey(m => m.OfficialId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.Title).IsUnique();
    }
}
