using Cobranca.Cidada.Domain.Interfaces.Repositories.Mandate;
using Cobranca.Cidada.Domain.Modules.Mandate.Entities;
using Cobranca.Cidada.Domain.Modules.Mandate.ValueObjects;
using Cobranca.Cidada.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using MandateAggregate = Cobranca.Cidada.Domain.Modules.Mandate.Aggregates.Mandate;

namespace Cobranca.Cidada.Infrastructure.Repositories.Mandate;

public class MandateRepository : IMandateRepository
{
    private readonly AppDbContext _context;

    public MandateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MandateAggregate?> GetByTitleAsync(string title, CancellationToken cancellationToken)
        => await _context.Mandates.FirstOrDefaultAsync(m => m.Title == title, cancellationToken);

    public async Task<MandateAggregate?> GetCurrentAsync(CancellationToken cancellationToken)
        => await _context.Mandates.AsNoTracking().OrderByDescending(m => m.StartDate).FirstOrDefaultAsync(cancellationToken);

    public async Task<Official?> GetOfficialAsync(Guid officialId, CancellationToken cancellationToken)
        => await _context.Officials.FirstOrDefaultAsync(o => o.Id == OfficialId.Create(officialId), cancellationToken);

    public async Task<Official?> GetOfficialByNameAsync(string name, CancellationToken cancellationToken)
        => await _context.Officials.FirstOrDefaultAsync(o => o.Name == name, cancellationToken);

    public async Task CreateOfficialAsync(Official official, CancellationToken cancellationToken)
        => await _context.Officials.AddAsync(official, cancellationToken);

    public async Task CreateAsync(MandateAggregate mandate, CancellationToken cancellationToken)
        => await _context.Mandates.AddAsync(mandate, cancellationToken);
}
