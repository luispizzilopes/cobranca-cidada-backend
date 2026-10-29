using Cobranca.Cidada.Application.Modules.Voting.Commands.RequestVoterCode;
using Cobranca.Cidada.Application.Modules.Voting.Commands.VerifyVoterCode;
using Cobranca.Cidada.Application.Modules.Voting.Requests;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using Cobranca.Cidada.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Cobranca.Cidada.Presentation.Controllers.V1;

/// <summary>Votar apenas com e-mail verificado, sem criar conta nem senha.</summary>
[Route("api/v{version:apiVersion}/voter")]
[ApiVersion("1.0")]
[ApiController]
[AllowAnonymous]
public class VoterController : ControllerBase
{
    private readonly IMediator _mediator;

    public VoterController(IMediator mediator) => _mediator = mediator;

    /// <summary>Envia um código de 6 dígitos ao e-mail para confirmar que a caixa postal é da pessoa</summary>
    [HttpPost("code")]
    [EnableRateLimiting("email-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RequestCode([FromBody] RequestVoterCodeRequest request)
        => (await _mediator.Send(new RequestVoterCodeCommand(request))).ToActionResult(this);

    /// <summary>Confere o código e devolve um token de votante (só permite votar e gerir os próprios votos)</summary>
    [HttpPost("verify")]
    [EnableRateLimiting("auth-anonymous")]
    [ProducesResponseType(typeof(VoterTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Verify([FromBody] VerifyVoterCodeRequest request)
        => (await _mediator.Send(new VerifyVoterCodeCommand(request))).ToActionResult(this);
}
