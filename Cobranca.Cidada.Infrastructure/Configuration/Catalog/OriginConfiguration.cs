using Cobranca.Cidada.Domain.Modules.Catalog.Entities;
using Cobranca.Cidada.Domain.Modules.Catalog.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobranca.Cidada.Infrastructure.Configuration.Catalog;

public class OriginConfiguration : IEntityTypeConfiguration<Origin>
{
    public void Configure(EntityTypeBuilder<Origin> builder)
    {
        builder.ToTable("Origins");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, value => OriginId.Create(value))
            .IsRequired();

        builder.Property(o => o.Code).HasMaxLength(100).IsRequired();
        builder.Property(o => o.Name).HasMaxLength(150).IsRequired();
        builder.Property(o => o.IsInOfficialPlan).IsRequired();

        builder.HasIndex(o => o.Code).IsUnique();
    }
}
