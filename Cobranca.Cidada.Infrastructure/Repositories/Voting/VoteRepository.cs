using Cobranca.Cidada.Domain.Interfaces.Repositories.Voting;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Voting.Aggregates;
using Cobranca.Cidada.Domain.Modules.Voting.Enums;
using Cobranca.Cidada.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cobranca.Cidada.Infrastructure.Repositories.Voting;

public class VoteRepository : IVoteRepository
{
    private readonly AppDbContext _context;

    public VoteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Vote?> GetAsync(string voterKey, Guid commitmentId, CancellationToken cancellationToken)
    {
        var id = CommitmentId.Create(commitmentId);
        return await _context.Votes.FirstOrDefaultAsync(v => v.VoterKey == voterKey && v.CommitmentId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Vote>> GetAllByVoterAsync(string voterKey, CancellationToken cancellationToken)
        => await _context.Votes.AsNoTracking().Where(v => v.VoterKey == voterKey).OrderBy(v => v.UpdatedAtUtc).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<VoteView>> GetViewsAsync(string voterKey, IEnumerable<Guid> commitmentIds, CancellationToken cancellationToken)
    {
        var ids = commitmentIds.Select(CommitmentId.Create).ToList();
        var votes = await _context.Votes.AsNoTracking()
            .Where(v => v.VoterKey == voterKey && ids.Contains(v.CommitmentId))
            .ToListAsync(cancellationToken);

        return votes.Select(v => new VoteView(v.CommitmentId.Value, v.Rating, v.Charge)).ToList();
    }

    public async Task<IReadOnlyList<VoteTally>> GetTalliesAsync(IEnumerable<Guid> commitmentIds, CancellationToken cancellationToken)
    {
        var ids = commitmentIds.Select(CommitmentId.Create).ToList();
        var rows = await _context.Votes.AsNoTracking()
            .Where(v => ids.Contains(v.CommitmentId))
            .GroupBy(v => v.CommitmentId)
            .Select(g => new
            {
                CommitmentId = g.Key,
                Charges = g.Count(v => v.Charge),
                Good = g.Count(v => v.Rating == VoteRating.Good),
                Bad = g.Count(v => v.Rating == VoteRating.Bad)
            })
            .ToListAsync(cancellationToken);

        return rows.Select(r => new VoteTally(r.CommitmentId.Value, r.Charges, r.Good, r.Bad)).ToList();
    }

    public async Task CreateAsync(Vote vote, CancellationToken cancellationToken)
        => await _context.Votes.AddAsync(vote, cancellationToken);

    public void Remove(Vote vote) => _context.Votes.Remove(vote);

    public async Task MoveAsync(string fromVoterKey, string toVoterKey, CancellationToken cancellationToken)
    {
        if (fromVoterKey == toVoterKey)
            return;

        var from = await _context.Votes.Where(v => v.VoterKey == fromVoterKey).ToListAsync(cancellationToken);
        var to = await _context.Votes.Where(v => v.VoterKey == toVoterKey).ToDictionaryAsync(v => v.CommitmentId.Value, cancellationToken);

        foreach (var vote in from)
        {
            if (to.TryGetValue(vote.CommitmentId.Value, out var existing))
            {
                if (vote.UpdatedAtUtc <= existing.UpdatedAtUtc)
                {
                    _context.Votes.Remove(vote);
                    continue;
                }

                _context.Votes.Remove(existing);
            }

            vote.Rekey(toVoterKey);
        }
    }

    public async Task<int> DeleteAllByVoterAsync(string voterKey, CancellationToken cancellationToken)
        => await _context.Votes.Where(v => v.VoterKey == voterKey).ExecuteDeleteAsync(cancellationToken);
}
