using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.UseCases.GetAdminCommitments;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Queries.GetAdminCommitments;

public class GetAdminCommitmentsQueryHandler : IRequestHandler<GetAdminCommitmentsQuery, ErrorOr<PaginedResult<AdminCommitmentSummaryResponse>>>
{
    private readonly GetAdminCommitmentsUseCase _useCase;

    public GetAdminCommitmentsQueryHandler(GetAdminCommitmentsUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<PaginedResult<AdminCommitmentSummaryResponse>>> Handle(GetAdminCommitmentsQuery query, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(query, cancellationToken);
}
