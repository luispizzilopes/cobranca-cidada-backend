using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Queries.GetCommitments;

public record GetCommitmentsQuery(
    string? Search,
    string? AreaCode,
    string? OriginCode,
    string? Status,
    bool? OnlyWithDeadline,
    string? Sort,
    int PageNumber,
    int PageSize,
    string? VoterKey = null,
    bool OnlyMine = false) : IRequest<ErrorOr<PaginedResult<CommitmentSummaryResponse>>>;
