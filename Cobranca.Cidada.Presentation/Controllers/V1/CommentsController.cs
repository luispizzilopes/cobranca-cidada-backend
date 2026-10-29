using Cobranca.Cidada.Application.Modules.Comment.Commands.AddComment;
using Cobranca.Cidada.Application.Modules.Comment.Commands.DeleteComment;
using Cobranca.Cidada.Application.Modules.Comment.Commands.DeleteMyComment;
using Cobranca.Cidada.Application.Modules.Comment.Queries.GetComments;
using Cobranca.Cidada.Application.Modules.Comment.Queries.GetRecentComments;
using Cobranca.Cidada.Application.Modules.Comment.Requests;
using Cobranca.Cidada.Application.Modules.Comment.Responses;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using Cobranca.Cidada.CrossCutting.IoC.Infrastructure.Identity;
using Cobranca.Cidada.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Cobranca.Cidada.Presentation.Controllers.V1;

/// <summary>Comentários e opiniões sobre os compromissos. Leitura pública; escrever exige conta ou e-mail verificado.</summary>
[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
[ApiController]
public class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lista os comentários de um compromisso publicado, do mais recente ao mais antigo</summary>
    /// <param name="id">Identificador público (ex.: ECON-006)</param>
    /// <param name="pageNumber">Página (a partir de 1)</param>
    /// <param name="pageSize">Itens por página (1 a 50)</param>
    [HttpGet("commitments/{id}/comments")]
    [AllowAnonymous]
    [EnableRateLimiting("public-read")]
    [ProducesResponseType(typeof(PaginedResult<CommentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(string id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        => (await _mediator.Send(new GetCommentsQuery(id, pageNumber, pageSize, HttpContext.GetVoterKey()))).ToActionResult(this);

    /// <summary>Comentários em destaque: o mais recente de cada compromisso publicado, sem respostas</summary>
    /// <param name="limit">Quantos comentários devolver (1 a 12)</param>
    [HttpGet("comments/recent")]
    [AllowAnonymous]
    [EnableRateLimiting("public-read")]
    [ProducesResponseType(typeof(IReadOnlyList<RecentCommentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 3)
        => (await _mediator.Send(new GetRecentCommentsQuery(limit))).ToActionResult(this);

    /// <summary>Publica um comentário em um compromisso</summary>
    [HttpPost("commitments/{id}/comments")]
    [Authorize(Policy = IdentityDependencyInjection.VoterPolicy)]
    [EnableRateLimiting("comment")]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Add(string id, [FromBody] AddCommentRequest request)
        => (await _mediator.Send(new AddCommentCommand(HttpContext.GetVoterKey()!, id, request))).ToActionResult(this);

    /// <summary>Apaga um comentário da própria pessoa</summary>
    [HttpDelete("comments/{commentId:guid}")]
    [Authorize(Policy = IdentityDependencyInjection.VoterPolicy)]
    [EnableRateLimiting("comment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid commentId)
        => (await _mediator.Send(new DeleteMyCommentCommand(HttpContext.GetVoterKey()!, commentId))).ToActionResult(this);

    /// <summary>Remove qualquer comentário (moderação). Só para administradores</summary>
    [HttpDelete("admin/comments/{commentId:guid}")]
    [Authorize(Policy = IdentityDependencyInjection.AdminPolicy)]
    [EnableRateLimiting("admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid commentId)
        => (await _mediator.Send(new DeleteCommentCommand(commentId, HttpContext.GetUserId()!))).ToActionResult(this);
}
