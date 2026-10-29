using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.UseCases.GetAdminCommitment;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Queries.GetAdminCommitment;

public class GetAdminCommitmentQueryHandler : IRequestHandler<GetAdminCommitmentQuery, ErrorOr<AdminCommitmentDetailResponse>>
{
    private readonly GetAdminCommitmentUseCase _useCase;

    public GetAdminCommitmentQueryHandler(GetAdminCommitmentUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<AdminCommitmentDetailResponse>> Handle(GetAdminCommitmentQuery query, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(query.Slug, cancellationToken);
}
