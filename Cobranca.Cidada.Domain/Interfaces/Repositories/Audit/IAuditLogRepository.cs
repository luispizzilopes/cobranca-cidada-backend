using Cobranca.Cidada.Domain.Modules.Audit.Entities;

namespace Cobranca.Cidada.Domain.Interfaces.Repositories.Audit;

public interface IAuditLogRepository
{
    Task CreateAsync(AuditLog log, CancellationToken cancellationToken);
}
