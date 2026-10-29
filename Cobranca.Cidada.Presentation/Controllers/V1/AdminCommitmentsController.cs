using Cobranca.Cidada.Application.Modules.Commitment.Commands.CreateCommitment;
using Cobranca.Cidada.Application.Modules.Commitment.Commands.RegisterCommitmentStatus;
using Cobranca.Cidada.Application.Modules.Commitment.Commands.SetCommitmentPublication;
using Cobranca.Cidada.Application.Modules.Commitment.Commands.UpdateCommitment;
using Cobranca.Cidada.Application.Modules.Commitment.Queries.GetAdminCommitment;
using Cobranca.Cidada.Application.Modules.Commitment.Queries.GetAdminCommitments;
using Cobranca.Cidada.Application.Modules.Commitment.Requests;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using Cobranca.Cidada.CrossCutting.IoC.Infrastructure.Identity;
using Cobranca.Cidada.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Cobranca.Cidada.Presentation.Controllers.V1;

/// <summary>Painel de administração: revisar e alterar compromissos, prazos, fontes e situação. Só para administradores.</summary>
[Route("api/v{version:apiVersion}/admin/commitments")]
[ApiVersion("1.0")]
[ApiController]
[Authorize(Policy = IdentityDependencyInjection.AdminPolicy)]
[EnableRateLimiting("admin")]
public class AdminCommitmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCommitmentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lista compromissos publicados ou não, com busca, filtros e paginação</summary>
    /// <param name="search">Texto a buscar no título, descrição e código</param>
    /// <param name="area">Código da área (ex.: ECON)</param>
    /// <param name="origin">Código da origem (ex.: plano-de-governo)</param>
    /// <param name="status">Situação de execução (rótulo em português ou nome do enum)</param>
    /// <param name="published">Quando informado, filtra por publicados (true) ou despublicados (false)</param>
    /// <param name="sort">area (padrão), code ou deadline</param>
    /// <param name="pageNumber">Página (a partir de 1)</param>
    /// <param name="pageSize">Itens por página (1 a 100)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginedResult<AdminCommitmentSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? area,
        [FromQuery] string? origin,
        [FromQuery] string? status,
        [FromQuery] bool? published,
        [FromQuery] string? sort,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
        => (await _mediator.Send(new GetAdminCommitmentsQuery(search, area, origin, status, published, sort, pageNumber, pageSize))).ToActionResult(this);

    /// <summary>Retorna tudo do compromisso, publicado ou não: conteúdo, fontes, histórico de situação e revisões</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AdminCommitmentDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
        => (await _mediator.Send(new GetAdminCommitmentQuery(id))).ToActionResult(this);

    /// <summary>Cria um compromisso (despublicado por padrão, situação "Não avaliado"). Sem identificador público, gera o próximo da área</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AdminCommitmentDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCommitmentRequest request)
    {
        var result = await _mediator.Send(new CreateCommitmentCommand(HttpContext.GetUserId()!, request));
        return result.IsError
            ? result.ToActionResult(this)
            : Created($"api/v1/admin/compromissos/{Uri.EscapeDataString(result.Value.Id)}", result.Value);
    }

    /// <summary>Altera o conteúdo (inclusive prazo e fontes). A versão anterior vira revisão; o motivo é obrigatório</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AdminCommitmentDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateCommitmentRequest request)
        => (await _mediator.Send(new UpdateCommitmentCommand(id, HttpContext.GetUserId()!, request))).ToActionResult(this);

    /// <summary>Registra uma nova situação de execução no histórico, com as regras editoriais (fonte obrigatória; nunca 'Descumprido' antes do prazo)</summary>
    [HttpPost("{id}/status")]
    [ProducesResponseType(typeof(AdminCommitmentDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegisterStatus(string id, [FromBody] RegisterStatusRequest request)
        => (await _mediator.Send(new RegisterCommitmentStatusCommand(id, HttpContext.GetUserId()!, request))).ToActionResult(this);

    /// <summary>Publica ou despublica o compromisso (despublicado deixa de aparecer na API pública)</summary>
    [HttpPut("{id}/publication")]
    [ProducesResponseType(typeof(AdminCommitmentDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPublication(string id, [FromBody] SetPublicationRequest request)
        => (await _mediator.Send(new SetCommitmentPublicationCommand(id, HttpContext.GetUserId()!, request.Published))).ToActionResult(this);
}
