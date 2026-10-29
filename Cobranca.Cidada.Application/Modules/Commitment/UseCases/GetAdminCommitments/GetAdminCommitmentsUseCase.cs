using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Commitment.Queries.GetAdminCommitments;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Commitment.Filters;
using Cobranca.Cidada.Application.Modules.Commitment.Mappers;
using Cobranca.Cidada.Application.Modules.Commitment.Parsers;

namespace Cobranca.Cidada.Application.Modules.Commitment.UseCases.GetAdminCommitments;

public class GetAdminCommitmentsUseCase : IUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminCommitmentsUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<PaginedResult<AdminCommitmentSummaryResponse>>> ExecuteAsync(GetAdminCommitmentsQuery query, CancellationToken cancellationToken = default)
    {
        var filters = new CommitmentFilters(
            string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            string.IsNullOrWhiteSpace(query.AreaCode) ? null : query.AreaCode.Trim().ToUpperInvariant(),
            string.IsNullOrWhiteSpace(query.OriginCode) ? null : query.OriginCode.Trim().ToLowerInvariant(),
            CommitmentQueryParser.ParseStatus(query.Status),
            OnlyWithDeadline: null,
            CommitmentQueryParser.ParseSort(query.Sort));

        var pagination = new PaginationParams { PageNumber = query.PageNumber, PageSize = query.PageSize };
        var result = await _unitOfWork.CommitmentRepository.GetAllAsync(filters, query.Published, pagination, cancellationToken);

        return new PaginedResult<AdminCommitmentSummaryResponse>
        {
            Items = result.Items.Select(c => c.ToAdminSummary()).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }
}
