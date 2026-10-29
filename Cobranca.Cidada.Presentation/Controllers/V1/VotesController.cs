using Cobranca.Cidada.Application.Modules.Voting.Commands.DeleteMyVotes;
using Cobranca.Cidada.Application.Modules.Voting.Commands.SetVote;
using Cobranca.Cidada.Application.Modules.Voting.Queries.GetMyVotes;
using Cobranca.Cidada.Application.Modules.Voting.Requests;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using Cobranca.Cidada.CrossCutting.IoC.Infrastructure.Identity;
using Cobranca.Cidada.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Cobranca.Cidada.Presentation.Controllers.V1;

/// <summary>"Cobrar" e "Boa/Má proposta". São opiniões e nunca alteram a situação de execução.</summary>
[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
[ApiController]
[Authorize(Policy = IdentityDependencyInjection.VoterPolicy)]
[EnableRateLimiting("vote")]
public class VotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VotesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Registra ou altera o voto da pessoa em um compromisso. Sem avaliação e sem "cobrar", o voto é desfeito</summary>
    [HttpPut("commitments/{id}/vote")]
    [ProducesResponseType(typeof(VoteStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetVote(string id, [FromBody] SetVoteRequest request)
        => (await _mediator.Send(new SetVoteCommand(HttpContext.GetVoterKey()!, id, request, HttpContext.GetClientIp()))).ToActionResult(this);

    /// <summary>Desfaz o voto da pessoa em um compromisso</summary>
    [HttpDelete("commitments/{id}/vote")]
    [ProducesResponseType(typeof(VoteStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveVote(string id)
        => (await _mediator.Send(new SetVoteCommand(HttpContext.GetVoterKey()!, id, new SetVoteRequest(null, false), HttpContext.GetClientIp()))).ToActionResult(this);

    /// <summary>Lista os votos da própria pessoa (serve também como exportação dos dados pessoais)</summary>
    [HttpGet("votes/mine")]
    [ProducesResponseType(typeof(IReadOnlyList<MyVoteItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine()
        => (await _mediator.Send(new GetMyVotesQuery(HttpContext.GetVoterKey()!))).ToActionResult(this);

    /// <summary>Apaga todos os votos da pessoa. Para quem votou só com e-mail, apaga também a verificação e encerra o token</summary>
    [HttpDelete("votes/mine")]
    [ProducesResponseType(typeof(DeleteVotesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteMine()
        => (await _mediator.Send(new DeleteMyVotesCommand(HttpContext.GetVoterKey()!, HttpContext.IsVoterOnly()))).ToActionResult(this);
}
