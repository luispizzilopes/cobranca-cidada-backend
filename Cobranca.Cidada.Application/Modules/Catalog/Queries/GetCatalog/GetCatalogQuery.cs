using ErrorOr;
using Cobranca.Cidada.Application.Modules.Catalog.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Catalog.Queries.GetCatalog;

public record GetCatalogQuery : IRequest<ErrorOr<CatalogResponse>>;
