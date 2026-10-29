using Cobranca.Cidada.Domain.Interfaces.Repositories.User;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Cobranca.Cidada.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cobranca.Cidada.Infrastructure.Repositories.User;

public class UserSessionRepository(AppDbContext context) : IUserSessionRepository
{
    public Task<UserSession?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
        => context.UserSessions.FirstOrDefaultAsync(session => session.RefreshTokenHash == tokenHash, cancellationToken);

    public Task<UserSession?> GetByIdAndUserIdAsync(Guid id, string userId, CancellationToken cancellationToken)
        => context.UserSessions.FirstOrDefaultAsync(session => session.Id == id && session.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<UserSession>> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
        => await context.UserSessions
            .AsNoTracking()
            .Where(session => session.UserId == userId && session.RevokedAtUtc == null && session.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(session => session.LastUsedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<bool> IsActiveAsync(Guid familyId, string userId, CancellationToken cancellationToken)
        => await context.UserSessions
            .AsNoTracking()
            .AnyAsync(
                session => session.FamilyId == familyId && session.UserId == userId &&
                           session.RevokedAtUtc == null && session.ExpiresAtUtc > DateTime.UtcNow,
                cancellationToken);

    public async Task CreateAsync(UserSession session, CancellationToken cancellationToken)
        => await context.UserSessions.AddAsync(session, cancellationToken);

    public async Task RevokeFamilyAsync(
        string userId,
        Guid familyId,
        string reason,
        CancellationToken cancellationToken)
    {
        var sessions = await context.UserSessions
            .Where(session => session.UserId == userId && session.FamilyId == familyId && session.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
            session.Revoke(reason);
    }

    public async Task RevokeAllAsync(
        string userId,
        string reason,
        Guid? exceptSessionId,
        CancellationToken cancellationToken)
    {
        var sessions = await context.UserSessions
            .Where(session => session.UserId == userId && session.RevokedAtUtc == null &&
                              (!exceptSessionId.HasValue || session.FamilyId != exceptSessionId.Value))
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
            session.Revoke(reason);
    }
}
