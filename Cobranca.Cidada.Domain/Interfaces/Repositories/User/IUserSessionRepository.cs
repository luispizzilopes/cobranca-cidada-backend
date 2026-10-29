using Cobranca.Cidada.Domain.Modules.User.Entities;

namespace Cobranca.Cidada.Domain.Interfaces.Repositories.User;

public interface IUserSessionRepository
{
    Task<UserSession?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task<UserSession?> GetByIdAndUserIdAsync(Guid id, string userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserSession>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<bool> IsActiveAsync(Guid id, string userId, CancellationToken cancellationToken);
    Task CreateAsync(UserSession session, CancellationToken cancellationToken);
    Task RevokeFamilyAsync(string userId, Guid familyId, string reason, CancellationToken cancellationToken);
    Task RevokeAllAsync(string userId, string reason, Guid? exceptSessionId, CancellationToken cancellationToken);
}
