using ErrorOr;
using Cobranca.Cidada.Application.Common;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Commitment.Queries.GetCommitments;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using Cobranca.Cidada.Domain.Interfaces.Caching;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Voting;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Commitment.Filters;

namespace Cobranca.Cidada.Application.Modules.Commitment.UseCases.GetCommitments;

public class GetCommitmentsUseCase : IUseCase
{
    private const string MostChargedSort = "mais-cobradas";
    private const string MostChargedCachePrefix = "commitments:most-charged:";
    private static readonly TimeSpan MostChargedCacheTime = TimeSpan.FromMinutes(5);

    private readonly IUnitOfWork _unitOfWork;
    private readonly VotePolicy _votePolicy;
    private readonly ICacheService _cache;

    public GetCommitmentsUseCase(IUnitOfWork unitOfWork, VotePolicy votePolicy, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _votePolicy = votePolicy;
        _cache = cache;
    }

    public async Task<ErrorOr<PaginedResult<CommitmentSummaryResponse>>> ExecuteAsync(GetCommitmentsQuery query, CancellationToken cancellationToken = default)
    {
        if (query.OnlyMine && query.VoterKey is null)
            return Error.Unauthorized("Auth.Required", "Entre ou confirme seu e-mail para ver suas cobranças.");

        var cacheKey = IsMostChargedPanel(query) ? $"{MostChargedCachePrefix}{query.PageNumber}:{query.PageSize}" : null;
        if (cacheKey is not null)
        {
            var cached = await _cache.GetAsync<CachedPage>(cacheKey, cancellationToken);
            if (cached is not null)
                return await WithMyVotesAsync(cached.Page, cached.Ids, query.VoterKey, cancellationToken);
        }

        var filters = new CommitmentFilters(
            string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            string.IsNullOrWhiteSpace(query.AreaCode) ? null : query.AreaCode.Trim().ToUpperInvariant(),
            string.IsNullOrWhiteSpace(query.OriginCode) ? null : query.OriginCode.Trim().ToLowerInvariant(),
            CommitmentQueryParser.ParseStatus(query.Status),
            query.OnlyWithDeadline,
            CommitmentQueryParser.ParseSort(query.Sort),
            query.OnlyMine ? query.VoterKey : null);

        var pagination = new PaginationParams { PageNumber = query.PageNumber, PageSize = query.PageSize };
        var result = await _unitOfWork.CommitmentRepository.GetPublishedAsync(filters, pagination, cancellationToken);

        var ids = result.Items.Select(c => c.Id.Value).ToList();
        var tallies = (await _unitOfWork.VoteRepository.GetTalliesAsync(ids, cancellationToken)).ToDictionary(t => t.CommitmentId);
        var mine = query.VoterKey is null || cacheKey is not null
            ? new Dictionary<Guid, VoteView>()
            : (await _unitOfWork.VoteRepository.GetViewsAsync(query.VoterKey, ids, cancellationToken)).ToDictionary(v => v.CommitmentId);

        var page = new PaginedResult<CommitmentSummaryResponse>
        {
            Items = result.Items.Select(c => c.ToSummary(
                _votePolicy.Summarize(tallies.GetValueOrDefault(c.Id.Value)),
                VotePolicy.ToMyVote(mine.GetValueOrDefault(c.Id.Value)))).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };

        if (cacheKey is null)
            return page;

        await _cache.SetAsync(cacheKey, new CachedPage(page, ids), MostChargedCacheTime, cancellationToken);
        return await WithMyVotesAsync(page, ids, query.VoterKey, cancellationToken);
    }

    private static bool IsMostChargedPanel(GetCommitmentsQuery query)
        => !query.OnlyMine
           && string.Equals(query.Sort?.Trim(), MostChargedSort, StringComparison.OrdinalIgnoreCase)
           && string.IsNullOrWhiteSpace(query.Search)
           && string.IsNullOrWhiteSpace(query.AreaCode)
           && string.IsNullOrWhiteSpace(query.OriginCode)
           && string.IsNullOrWhiteSpace(query.Status)
           && query.OnlyWithDeadline is null;

    private async Task<PaginedResult<CommitmentSummaryResponse>> WithMyVotesAsync(
        PaginedResult<CommitmentSummaryResponse> page, List<Guid> ids, string? voterKey, CancellationToken cancellationToken)
    {
        if (voterKey is null || ids.Count == 0)
            return page;

        var mine = (await _unitOfWork.VoteRepository.GetViewsAsync(voterKey, ids, cancellationToken)).ToDictionary(v => v.CommitmentId);

        return new PaginedResult<CommitmentSummaryResponse>
        {
            Items = page.Items.Select((item, i) => item with { MyVote = VotePolicy.ToMyVote(mine.GetValueOrDefault(ids[i])) }).ToList(),
            TotalCount = page.TotalCount,
            PageNumber = page.PageNumber,
            PageSize = page.PageSize
        };
    }

    /// <summary>Página sem o voto da pessoa, com os ids na mesma ordem dos itens (para buscar o voto depois).</summary>
    private sealed record CachedPage(PaginedResult<CommitmentSummaryResponse> Page, List<Guid> Ids);
}
