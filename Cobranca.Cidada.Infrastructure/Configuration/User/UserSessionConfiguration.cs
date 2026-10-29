using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobranca.Cidada.Infrastructure.Configuration.User;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");
        builder.HasKey(session => session.Id);

        builder.Property(session => session.UserId).HasMaxLength(450).IsRequired();
        builder.Property(session => session.FamilyId).IsRequired();
        builder.Property(session => session.RefreshTokenHash).HasMaxLength(64).IsRequired();
        builder.Property(session => session.DeviceName).HasMaxLength(160).IsRequired();
        builder.Property(session => session.UserAgent).HasMaxLength(512);
        builder.Property(session => session.IpAddress).HasMaxLength(64);
        builder.Property(session => session.RevocationReason).HasMaxLength(256);

        builder.HasIndex(session => session.RefreshTokenHash).IsUnique();
        builder.HasIndex(session => new { session.UserId, session.FamilyId });
        builder.HasIndex(session => new { session.UserId, session.RevokedAtUtc, session.ExpiresAtUtc });

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
