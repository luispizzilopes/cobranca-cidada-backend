using ErrorOr;
using Cobranca.Cidada.Application.Modules.Catalog.Responses;
using Cobranca.Cidada.Application.Modules.Catalog.UseCases.GetCatalog;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Catalog.Queries.GetCatalog;

public class GetCatalogQueryHandler : IRequestHandler<GetCatalogQuery, ErrorOr<CatalogResponse>>
{
    private readonly GetCatalogUseCase _useCase;

    public GetCatalogQueryHandler(GetCatalogUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<CatalogResponse>> Handle(GetCatalogQuery query, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(cancellationToken);
}
