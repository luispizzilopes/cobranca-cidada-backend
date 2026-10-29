using Cobranca.Cidada.Application.Modules.Catalog.Queries.GetCatalog;
using Cobranca.Cidada.Application.Modules.Catalog.Responses;
using Cobranca.Cidada.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Cobranca.Cidada.Presentation.Controllers.V1;

[Route("api/v{version:apiVersion}/catalog")]
[ApiVersion("1.0")]
[ApiController]
[EnableRateLimiting("public-read")]
[AllowAnonymous]
public class CatalogController : ControllerBase
{
    private readonly IMediator _mediator;

    public CatalogController(IMediator mediator) => _mediator = mediator;

    /// <summary>Retorna áreas, origens e situações disponíveis para filtros</summary>
    [HttpGet]
    [ProducesResponseType(typeof(CatalogResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var result = await _mediator.Send(new GetCatalogQuery());
        return result.ToActionResult(this);
    }
}
