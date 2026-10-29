using Cobranca.Cidada.BuildingBlocks.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Cobranca.Cidada.Domain.Modules.User.Entities;

public class AppUser : IdentityUser, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public int LockoutLevel { get; set; }

    /// <summary>Administrador do painel de curadoria. Só se altera direto no banco (ou pelo comando <c>set-admin</c>); nenhum endpoint concede.</summary>
    public bool IsAdmin { get; set; }

    /// <summary>Registro do consentimento (LGPD): quando e qual versão da política de privacidade foi aceita.</summary>
    public DateTime? ConsentAcceptedAtUtc { get; set; }
    public string? ConsentVersion { get; set; }

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;

    public void SetCreatedAt(DateTime timestampUtc) => CreatedAtUtc = timestampUtc;
    public void SetUpdatedAt(DateTime timestampUtc) => UpdatedAtUtc = timestampUtc;
}
