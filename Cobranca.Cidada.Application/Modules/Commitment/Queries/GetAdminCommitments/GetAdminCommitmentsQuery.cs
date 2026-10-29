using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Queries.GetAdminCommitments;

public record GetAdminCommitmentsQuery(
    string? Search,
    string? AreaCode,
    string? OriginCode,
    string? Status,
    bool? Published,
    string? Sort,
    int PageNumber,
    int PageSize) : IRequest<ErrorOr<PaginedResult<AdminCommitmentSummaryResponse>>>;
