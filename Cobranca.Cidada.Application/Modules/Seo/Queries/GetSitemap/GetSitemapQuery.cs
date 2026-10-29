using ErrorOr;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Seo.Queries.GetSitemap;

public record GetSitemapQuery : IRequest<ErrorOr<string>>;
