using Cobranca.Cidada.Application.Modules.Catalog.Commands.RenameArea;
using Cobranca.Cidada.Application.Modules.Catalog.Commands.RenameOrigin;
using Cobranca.Cidada.Application.Modules.Catalog.Requests;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.CrossCutting.IoC.Infrastructure.Identity;
using Cobranca.Cidada.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Cobranca.Cidada.Presentation.Controllers.V1;

/// <summary>Áreas e origens (tabelas de apoio). O código é fixo; o nome exibido pode ser corrigido. Só para administradores.</summary>
[Route("api/v{version:apiVersion}/admin/catalog")]
[ApiVersion("1.0")]
[ApiController]
[Authorize(Policy = IdentityDependencyInjection.AdminPolicy)]
[EnableRateLimiting("admin")]
public class AdminCatalogController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCatalogController(IMediator mediator) => _mediator = mediator;

    /// <summary>Renomeia uma área</summary>
    [HttpPut("areas/{code}")]
    [ProducesResponseType(typeof(LookupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RenameArea(string code, [FromBody] RenameCatalogItemRequest request)
        => (await _mediator.Send(new RenameAreaCommand(code, HttpContext.GetUserId()!, request.Name))).ToActionResult(this);

    /// <summary>Renomeia uma origem</summary>
    [HttpPut("origins/{code}")]
    [ProducesResponseType(typeof(LookupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RenameOrigin(string code, [FromBody] RenameCatalogItemRequest request)
        => (await _mediator.Send(new RenameOriginCommand(code, HttpContext.GetUserId()!, request.Name))).ToActionResult(this);
}
