using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.UseCases.GetCommitments;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Queries.GetCommitments;

public class GetCommitmentsQueryHandler : IRequestHandler<GetCommitmentsQuery, ErrorOr<PaginedResult<CommitmentSummaryResponse>>>
{
    private readonly GetCommitmentsUseCase _useCase;

    public GetCommitmentsQueryHandler(GetCommitmentsUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<PaginedResult<CommitmentSummaryResponse>>> Handle(GetCommitmentsQuery query, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(query, cancellationToken);
}
