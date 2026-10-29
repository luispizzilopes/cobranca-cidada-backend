using Cobranca.Cidada.Domain.Modules.Moderation.Entities;
using Cobranca.Cidada.Domain.Modules.Moderation.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobranca.Cidada.Infrastructure.Configuration.Moderation;

public class ProfanityWordConfiguration : IEntityTypeConfiguration<ProfanityWord>
{
    public void Configure(EntityTypeBuilder<ProfanityWord> builder)
    {
        builder.ToTable("ProfanityWords");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasConversion(id => id.Value, value => ProfanityWordId.Create(value))
            .IsRequired();

        builder.Property(w => w.Word).HasMaxLength(ProfanityWord.MaxLength).IsRequired();
        builder.Property(w => w.Language).HasMaxLength(2).IsFixedLength().IsRequired();
        builder.Property(w => w.IsActive).IsRequired();
        builder.Property(w => w.CreatedAtUtc).IsRequired();

        builder.HasIndex(w => w.Word).IsUnique();
    }
}
