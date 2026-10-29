using Cobranca.Cidada.Domain.Modules.Mandate.Entities;
using Cobranca.Cidada.Domain.Modules.Mandate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobranca.Cidada.Infrastructure.Configuration.Mandate;

public class OfficialConfiguration : IEntityTypeConfiguration<Official>
{
    public void Configure(EntityTypeBuilder<Official> builder)
    {
        builder.ToTable("Officials");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, value => OfficialId.Create(value))
            .IsRequired();

        builder.Property(o => o.Name).HasMaxLength(200).IsRequired();
        builder.Property(o => o.Party).HasMaxLength(30);

        builder.HasIndex(o => o.Name);
    }
}
