using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.UseCases.GetCommitmentBySlug;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Queries.GetCommitmentBySlug;

public class GetCommitmentBySlugQueryHandler : IRequestHandler<GetCommitmentBySlugQuery, ErrorOr<CommitmentDetailResponse>>
{
    private readonly GetCommitmentBySlugUseCase _useCase;

    public GetCommitmentBySlugQueryHandler(GetCommitmentBySlugUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<CommitmentDetailResponse>> Handle(GetCommitmentBySlugQuery query, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(query.Slug, query.VoterKey, cancellationToken);
}
