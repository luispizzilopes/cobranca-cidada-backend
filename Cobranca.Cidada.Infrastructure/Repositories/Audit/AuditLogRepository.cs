using Cobranca.Cidada.Domain.Interfaces.Repositories.Audit;
using Cobranca.Cidada.Domain.Modules.Audit.Entities;
using Cobranca.Cidada.Infrastructure.Context;

namespace Cobranca.Cidada.Infrastructure.Repositories.Audit;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(AuditLog log, CancellationToken cancellationToken)
        => await _context.AuditLogs.AddAsync(log, cancellationToken);
}
