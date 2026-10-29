using Cobranca.Cidada.Domain.Interfaces.Repositories.Voting;
using Cobranca.Cidada.Domain.Modules.Voting.Entities;
using Cobranca.Cidada.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cobranca.Cidada.Infrastructure.Repositories.Voting;

public class VoterIdentityRepository : IVoterIdentityRepository
{
    private readonly AppDbContext _context;

    public VoterIdentityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<VoterIdentity?> GetAsync(string voterKey, CancellationToken cancellationToken)
        => await _context.VoterIdentities.FirstOrDefaultAsync(v => v.Id == voterKey, cancellationToken);

    public async Task CreateAsync(VoterIdentity identity, CancellationToken cancellationToken)
        => await _context.VoterIdentities.AddAsync(identity, cancellationToken);

    public async Task<bool> IsTokenValidAsync(string voterKey, DateTime issuedAtUtc, CancellationToken cancellationToken)
        => await _context.VoterIdentities.AsNoTracking()
            .AnyAsync(v => v.Id == voterKey && v.TokensValidAfterUtc <= issuedAtUtc, cancellationToken);

    public async Task DeleteAsync(string voterKey, CancellationToken cancellationToken)
        => await _context.VoterIdentities.Where(v => v.Id == voterKey).ExecuteDeleteAsync(cancellationToken);
}
