using Cobranca.Cidada.Domain.Interfaces.Repositories.Catalog;
using Cobranca.Cidada.Domain.Modules.Catalog.Entities;
using Cobranca.Cidada.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cobranca.Cidada.Infrastructure.Repositories.Catalog;

public class OriginRepository : IOriginRepository
{
    private readonly AppDbContext _context;

    public OriginRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Origin>> GetAllAsync(CancellationToken cancellationToken)
        => await _context.Origins.OrderByDescending(o => o.IsInOfficialPlan).ThenBy(o => o.Name).ToListAsync(cancellationToken);

    public async Task<Origin?> GetByCodeAsync(string code, CancellationToken cancellationToken)
        => await _context.Origins.FirstOrDefaultAsync(o => o.Code == code, cancellationToken);

    public async Task CreateAsync(Origin origin, CancellationToken cancellationToken)
        => await _context.Origins.AddAsync(origin, cancellationToken);
}
