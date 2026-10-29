using Cobranca.Cidada.Domain.Modules.Catalog.Entities;
using Cobranca.Cidada.Domain.Modules.Catalog.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobranca.Cidada.Infrastructure.Configuration.Catalog;

public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("Areas");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => AreaId.Create(value))
            .IsRequired();

        builder.Property(a => a.Code).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Name).HasMaxLength(150).IsRequired();

        builder.HasIndex(a => a.Code).IsUnique();
    }
}
