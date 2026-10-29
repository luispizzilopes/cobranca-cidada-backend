using ErrorOr;
using Cobranca.Cidada.Application.Modules.Seo.UseCases.GetSitemap;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Seo.Queries.GetSitemap;

public class GetSitemapQueryHandler : IRequestHandler<GetSitemapQuery, ErrorOr<string>>
{
    private readonly GetSitemapUseCase _useCase;

    public GetSitemapQueryHandler(GetSitemapUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<string>> Handle(GetSitemapQuery query, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(cancellationToken);
}
