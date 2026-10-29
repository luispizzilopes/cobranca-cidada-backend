using ErrorOr;
using Cobranca.Cidada.Application.Modules.Seo.UseCases.GetSeoFragment;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Seo.Queries.GetSeoFragment;

public class GetSeoFragmentQueryHandler : IRequestHandler<GetSeoFragmentQuery, ErrorOr<string>>
{
    private readonly GetSeoFragmentUseCase _useCase;

    public GetSeoFragmentQueryHandler(GetSeoFragmentUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<string>> Handle(GetSeoFragmentQuery query, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(query.Path, query.Part, cancellationToken);
}
