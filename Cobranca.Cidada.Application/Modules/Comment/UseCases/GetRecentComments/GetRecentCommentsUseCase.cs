using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Comment.Queries.GetRecentComments;
using Cobranca.Cidada.Application.Modules.Comment.Responses;
using Cobranca.Cidada.Domain.Interfaces.Caching;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;

namespace Cobranca.Cidada.Application.Modules.Comment.UseCases.GetRecentComments;

public class GetRecentCommentsUseCase : IUseCase
{
    public const string CachePrefix = "comments:recent:";
    private static readonly TimeSpan CacheTime = TimeSpan.FromMinutes(5);

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public GetRecentCommentsUseCase(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<ErrorOr<IReadOnlyList<RecentCommentResponse>>> ExecuteAsync(GetRecentCommentsQuery query, CancellationToken cancellationToken = default)
    {
        var key = $"{CachePrefix}{query.Limit}";
        var cached = await _cache.GetAsync<List<RecentCommentResponse>>(key, cancellationToken);
        if (cached is not null)
            return cached;

        var recent = await _unitOfWork.CommentRepository.GetRecentAsync(query.Limit, cancellationToken);

        var response = recent
            .Select(r => new RecentCommentResponse(r.Comment.Id.Value, r.Comment.Text, r.Comment.CreatedAtUtc, r.CommitmentSlug, r.CommitmentTitle))
            .ToList();

        await _cache.SetAsync(key, response, CacheTime, cancellationToken);
        return response;
    }
}
