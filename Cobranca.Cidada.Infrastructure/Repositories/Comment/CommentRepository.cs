using Cobranca.Cidada.BuildingBlocks.Pagination;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Comment;
using Cobranca.Cidada.Domain.Modules.Comment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Cobranca.Cidada.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using CommentAggregate = Cobranca.Cidada.Domain.Modules.Comment.Aggregates.Comment;

namespace Cobranca.Cidada.Infrastructure.Repositories.Comment;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginedResult<CommentAggregate>> GetByCommitmentAsync(Guid commitmentId, PaginationParams pagination, CancellationToken cancellationToken)
    {
        var id = CommitmentId.Create(commitmentId);
        var query = _context.Comments.AsNoTracking().Where(c => c.CommitmentId == id && c.ParentId == null);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((pagination.PageNumber - 1) * pagination.FinalSize)
            .Take(pagination.FinalSize)
            .ToListAsync(cancellationToken);

        return new PaginedResult<CommentAggregate>
        {
            Items = items,
            TotalCount = total,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.FinalSize
        };
    }

    public async Task<IReadOnlyList<CommentAggregate>> GetRepliesAsync(IEnumerable<Guid> rootIds, CancellationToken cancellationToken)
    {
        var ids = rootIds.Select(CommentId.Create).ToList();
        return await _context.Comments.AsNoTracking()
            .Where(c => c.ParentId != null && ids.Contains(c.ParentId))
            .OrderBy(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RecentCommentRef>> GetRecentAsync(int limit, CancellationToken cancellationToken)
    {
        var rows = await (
                from c in _context.Comments.AsNoTracking()
                join m in _context.Commitments.AsNoTracking() on c.CommitmentId equals m.Id
                where c.ParentId == null
                    && m.IsPublished
                    && c.CreatedAtUtc == _context.Comments
                        .Where(o => o.ParentId == null && o.CommitmentId == c.CommitmentId)
                        .Max(o => o.CreatedAtUtc)
                orderby c.CreatedAtUtc descending
                select new { Comment = c, m.Slug, m.Title })
            .Take(limit)
            .ToListAsync(cancellationToken);

        return rows.Select(r => new RecentCommentRef(r.Comment, r.Slug, r.Title)).ToList();
    }

    public async Task<CommentAggregate?> GetAsync(Guid commentId, CancellationToken cancellationToken)
    {
        var id = CommentId.Create(commentId);
        return await _context.Comments.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<int> CountByVoterAsync(string voterKey, Guid commitmentId, CancellationToken cancellationToken)
    {
        var id = CommitmentId.Create(commitmentId);
        return await _context.Comments.CountAsync(c => c.VoterKey == voterKey && c.CommitmentId == id && c.ParentId == null, cancellationToken);
    }

    public async Task CreateAsync(CommentAggregate comment, CancellationToken cancellationToken)
        => await _context.Comments.AddAsync(comment, cancellationToken);

    public void Remove(CommentAggregate comment) => _context.Comments.Remove(comment);

    public async Task MoveAsync(string fromVoterKey, string toVoterKey, CancellationToken cancellationToken)
    {
        if (fromVoterKey == toVoterKey)
            return;

        var comments = await _context.Comments.Where(c => c.VoterKey == fromVoterKey).ToListAsync(cancellationToken);
        foreach (var comment in comments)
            comment.Rekey(toVoterKey);
    }

    public async Task<int> DeleteAllByVoterAsync(string voterKey, CancellationToken cancellationToken)
        => await _context.Comments.Where(c => c.VoterKey == voterKey).ExecuteDeleteAsync(cancellationToken);
}
