using Cobranca.Cidada.BuildingBlocks.Pagination;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Commitment;
using Cobranca.Cidada.Domain.Modules.Commitment.Filters;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Voting.Enums;
using Cobranca.Cidada.Infrastructure.Context;
using Cobranca.Cidada.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CommitmentAggregate = Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment;

namespace Cobranca.Cidada.Infrastructure.Repositories.Commitment;

public class CommitmentRepository : ICommitmentRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<CommitmentRepository> _logger;

    public CommitmentRepository(AppDbContext context, ILogger<CommitmentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CommitmentAggregate?> GetBySlugAsync(string slug, bool onlyPublished, CancellationToken cancellationToken)
    {
        return await _context.Commitments
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Area)
            .Include(c => c.Origin)
            .Include(c => c.Sources)
            .Include(c => c.StatusEntries)
            .Where(c => c.Slug == slug && (!onlyPublished || c.IsPublished))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CommitmentAggregate?> GetForAdminAsync(string slug, CancellationToken cancellationToken)
    {
        return await _context.Commitments
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Area)
            .Include(c => c.Origin)
            .Include(c => c.Sources)
            .Include(c => c.StatusEntries)
            .Include(c => c.Revisions)
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<CommitmentAggregate?> GetForUpdateAsync(string slug, CancellationToken cancellationToken)
    {
        return await _context.Commitments
            .AsSplitQuery()
            .Include(c => c.Sources)
            .Include(c => c.StatusEntries)
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<CommitmentAggregate?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken)
    {
        return await _context.Commitments
            .Include(c => c.Sources)
            .FirstOrDefaultAsync(c => c.ExternalId == externalId, cancellationToken);
    }

    public async Task<HashSet<string>> GetExistingExternalIdsAsync(IEnumerable<string> externalIds, CancellationToken cancellationToken)
    {
        var ids = externalIds.Distinct().ToList();
        var found = await _context.Commitments
            .AsNoTracking()
            .Where(c => c.ExternalId != null && ids.Contains(c.ExternalId))
            .Select(c => c.ExternalId!)
            .ToListAsync(cancellationToken);

        return found.ToHashSet(StringComparer.Ordinal);
    }

    public async Task<HashSet<string>> GetExistingSlugsAsync(CancellationToken cancellationToken)
    {
        var slugs = await _context.Commitments
            .AsNoTracking()
            .Select(c => c.Slug)
            .ToListAsync(cancellationToken);

        return slugs.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public Task<PaginedResult<CommitmentAggregate>> GetPublishedAsync(
        CommitmentFilters filters,
        PaginationParams pagination,
        CancellationToken cancellationToken)
        => GetAllAsync(filters, published: true, pagination, cancellationToken);

    public async Task<PaginedResult<CommitmentAggregate>> GetAllAsync(
        CommitmentFilters filters,
        bool? published,
        PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var query = _context.Commitments
            .AsNoTracking()
            .Include(c => c.Area)
            .Include(c => c.Origin)
            .AsQueryable();

        if (published is { } isPublished)
            query = query.Where(c => c.IsPublished == isPublished);

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var pattern = $"%{EscapeLike(filters.Search)}%";
            query = query.Where(c =>
                EF.Functions.ILike(c.Title, pattern)
                || EF.Functions.ILike(c.Description, pattern)
                || (c.ShortDescription != null && EF.Functions.ILike(c.ShortDescription, pattern))
                || EF.Functions.ILike(c.Slug, pattern));
        }

        if (filters.AreaCode is not null)
            query = query.Where(c => c.Area.Code == filters.AreaCode);

        if (filters.OriginCode is not null)
            query = query.Where(c => c.Origin.Code == filters.OriginCode);

        if (filters.Status is not null)
            query = query.Where(c => c.CurrentStatus == filters.Status);

        if (filters.OnlyWithDeadline == true)
            query = query.Where(c => c.DeadlineText != null);

        if (filters.OnlyChargedByVoterKey is { } voterKey)
            query = query.Where(c => _context.Votes.Any(v => v.CommitmentId == c.Id && v.VoterKey == voterKey && v.Charge));

        query = filters.Sort switch
        {
            CommitmentSort.MostCharged => query
                .OrderByDescending(c => _context.Votes.Count(v => v.CommitmentId == c.Id && v.Charge))
                .ThenBy(c => c.Slug),
            CommitmentSort.BestRated => query
                .OrderByDescending(c => _context.Votes.Count(v => v.CommitmentId == c.Id && v.Rating == VoteRating.Good)
                                        - _context.Votes.Count(v => v.CommitmentId == c.Id && v.Rating == VoteRating.Bad))
                .ThenBy(c => c.Slug),
            CommitmentSort.WorstRated => query
                .OrderBy(c => _context.Votes.Count(v => v.CommitmentId == c.Id && v.Rating == VoteRating.Good)
                              - _context.Votes.Count(v => v.CommitmentId == c.Id && v.Rating == VoteRating.Bad))
                .ThenBy(c => c.Slug),
            CommitmentSort.Code => query.OrderBy(c => c.Slug),
            CommitmentSort.Deadline => query.OrderBy(c => c.DeadlineText == null).ThenBy(c => c.DeadlineText).ThenBy(c => c.Slug),
            _ => query.OrderBy(c => c.Area.Name).ThenBy(c => c.Slug)
        };

        return await query.PaginationAsync(pagination, cancellationToken);
    }

    public async Task<IReadOnlyList<PublishedCommitmentRef>> GetPublishedRefsAsync(CancellationToken cancellationToken)
    {
        return await _context.Commitments.AsNoTracking()
            .Where(c => c.IsPublished)
            .OrderBy(c => c.Slug)
            .Select(c => new PublishedCommitmentRef(c.Slug, c.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid?> GetPublishedIdBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var id = await _context.Commitments.AsNoTracking()
            .Where(c => c.Slug == slug && c.IsPublished)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return id?.Value;
    }

    public async Task<IReadOnlyDictionary<Guid, (string Slug, string Title)>> GetHeadersByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var typedIds = ids.Select(CommitmentId.Create).ToList();
        var rows = await _context.Commitments.AsNoTracking()
            .Where(c => typedIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Slug, c.Title })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(r => r.Id.Value, r => (r.Slug, r.Title));
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken)
        => await _context.Commitments.CountAsync(cancellationToken);

    public async Task CreateAsync(CommitmentAggregate commitment, CancellationToken cancellationToken)
    {
        try
        {
            await _context.Commitments.AddAsync(commitment, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu um erro ao criar o compromisso. Erro: {Message}", ex.Message);
            throw;
        }
    }

    public Task UpdateAsync(CommitmentAggregate commitment, CancellationToken cancellationToken)
    {
        try
        {
            _context.Commitments.Update(commitment);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu um erro ao atualizar o compromisso. Erro: {Message}", ex.Message);
            throw;
        }
    }

    private static string EscapeLike(string value)
        => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
