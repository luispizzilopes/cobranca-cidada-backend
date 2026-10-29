using Cobranca.Cidada.Domain.Modules.Voting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobranca.Cidada.Infrastructure.Configuration.Voting;

public class VoterIdentityConfiguration : IEntityTypeConfiguration<VoterIdentity>
{
    public void Configure(EntityTypeBuilder<VoterIdentity> builder)
    {
        builder.ToTable("VoterIdentities");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasMaxLength(64).IsFixedLength().ValueGeneratedNever();
        builder.Property(v => v.VerifiedAtUtc).IsRequired();
        builder.Property(v => v.LastVerifiedAtUtc).IsRequired();
        builder.Property(v => v.TokensValidAfterUtc).IsRequired();
    }
}
