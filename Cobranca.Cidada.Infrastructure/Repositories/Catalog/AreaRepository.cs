using Cobranca.Cidada.Domain.Interfaces.Repositories.Catalog;
using Cobranca.Cidada.Domain.Modules.Catalog.Entities;
using Cobranca.Cidada.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cobranca.Cidada.Infrastructure.Repositories.Catalog;

public class AreaRepository : IAreaRepository
{
    private readonly AppDbContext _context;

    public AreaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Area>> GetAllAsync(CancellationToken cancellationToken)
        => await _context.Areas.OrderBy(a => a.Name).ToListAsync(cancellationToken);

    public async Task<Area?> GetByCodeAsync(string code, CancellationToken cancellationToken)
        => await _context.Areas.FirstOrDefaultAsync(a => a.Code == code, cancellationToken);

    public async Task CreateAsync(Area area, CancellationToken cancellationToken)
        => await _context.Areas.AddAsync(area, cancellationToken);
}
