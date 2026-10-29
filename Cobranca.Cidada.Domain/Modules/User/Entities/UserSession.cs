using Cobranca.Cidada.BuildingBlocks.Common.Class.Base;

namespace Cobranca.Cidada.Domain.Modules.User.Entities;

public class UserSession : Entity<Guid>
{
    public string UserId { get; private set; } = string.Empty;
    public Guid FamilyId { get; private set; }
    public string RefreshTokenHash { get; private set; } = string.Empty;
    public string DeviceName { get; private set; } = string.Empty;
    public string? UserAgent { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime LastUsedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? RevocationReason { get; private set; }
    public Guid? ReplacedBySessionId { get; private set; }

    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;

    public UserSession() { }

    public static UserSession Create(
        string userId,
        string refreshTokenHash,
        string deviceName,
        string? userAgent,
        string? ipAddress,
        DateTime expiresAtUtc,
        Guid? familyId = null)
    {
        var now = DateTime.UtcNow;
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FamilyId = familyId ?? Guid.Empty,
            RefreshTokenHash = refreshTokenHash,
            DeviceName = string.IsNullOrWhiteSpace(deviceName) ? "Dispositivo desconhecido" : deviceName.Trim(),
            UserAgent = userAgent,
            IpAddress = ipAddress,
            CreatedAtUtc = now,
            LastUsedAtUtc = now,
            ExpiresAtUtc = expiresAtUtc
        };

        if (session.FamilyId == Guid.Empty)
            session.FamilyId = session.Id;

        return session;
    }

    public void Rotate(Guid replacementSessionId)
        => Revoke("Rotação do token de atualização.", replacementSessionId);

    public void Revoke(string reason, Guid? replacementSessionId = null)
    {
        if (RevokedAtUtc is not null)
            return;

        RevokedAtUtc = DateTime.UtcNow;
        RevocationReason = reason;
        ReplacedBySessionId = replacementSessionId;
    }

    public void Touch(string? ipAddress)
    {
        LastUsedAtUtc = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(ipAddress))
            IpAddress = ipAddress;
    }
}
