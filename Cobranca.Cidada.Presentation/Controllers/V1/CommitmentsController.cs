using Cobranca.Cidada.Application.Modules.Commitment.Queries.GetCommitmentBySlug;
using Cobranca.Cidada.Application.Modules.Commitment.Queries.GetCommitments;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using Cobranca.Cidada.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Cobranca.Cidada.Presentation.Controllers.V1;

[Route("api/v{version:apiVersion}/commitments")]
[ApiVersion("1.0")]
[ApiController]
[EnableRateLimiting("public-read")]
[AllowAnonymous]
public class CommitmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommitmentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lista compromissos publicados, com busca, filtros e paginação feitos no servidor</summary>
    /// <param name="search">Texto a buscar no título, descrição e código</param>
    /// <param name="area">Código da área (ex.: ECON)</param>
    /// <param name="origin">Código da origem (ex.: plano-de-governo)</param>
    /// <param name="status">Situação de execução (rótulo em português ou nome do enum)</param>
    /// <param name="hasDeadline">Quando verdadeiro, retorna apenas compromissos com prazo informado</param>
    /// <param name="sort">area (padrão), code, deadline, mais-cobradas, melhor-avaliadas ou pior-avaliadas</param>
    /// <param name="myCharges">Quando verdadeiro, só os compromissos que a pessoa marcou como "cobrar" (exige token)</param>
    /// <param name="pageNumber">Página (a partir de 1)</param>
    /// <param name="pageSize">Itens por página (1 a 100)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginedResult<CommitmentSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? area,
        [FromQuery] string? origin,
        [FromQuery] string? status,
        [FromQuery] bool? hasDeadline,
        [FromQuery] string? sort,
        [FromQuery] bool myCharges = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetCommitmentsQuery(search, area, origin, status, hasDeadline, sort, pageNumber, pageSize, HttpContext.GetVoterKey(), myCharges));
        return result.ToActionResult(this);
    }

    /// <summary>Retorna o detalhe de um compromisso publicado, com fontes e histórico de situação</summary>
    /// <param name="id">Identificador público (ex.: ECON-006)</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CommitmentDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _mediator.Send(new GetCommitmentBySlugQuery(id, HttpContext.GetVoterKey()));
        return result.ToActionResult(this);
    }
}
