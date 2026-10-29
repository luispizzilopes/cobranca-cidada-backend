using Cobranca.Cidada.BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Cobranca.Cidada.Infrastructure.Extensions;

public static class QueryableExtension
{
    public static async Task<PaginedResult<T>> PaginationAsync<T>(
        this IQueryable<T> query,
        PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.FinalSize)
            .Take(pagination.FinalSize)
            .ToListAsync(cancellationToken);

        return new PaginedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.FinalSize
        };
    }
}
