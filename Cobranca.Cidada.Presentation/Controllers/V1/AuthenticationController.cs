using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Commands.ChangePassword;
using Cobranca.Cidada.Application.Modules.Auth.Commands.ConfirmEmailChange;
using Cobranca.Cidada.Application.Modules.Auth.Commands.ConfirmPasswordReset;
using Cobranca.Cidada.Application.Modules.Auth.Commands.DeleteAccount;
using Cobranca.Cidada.Application.Modules.Auth.Commands.GoogleLogin;
using Cobranca.Cidada.Application.Modules.Auth.Commands.Login;
using Cobranca.Cidada.Application.Modules.Auth.Commands.Logout;
using Cobranca.Cidada.Application.Modules.Auth.Commands.RefreshSession;
using Cobranca.Cidada.Application.Modules.Auth.Commands.Register;
using Cobranca.Cidada.Application.Modules.Auth.Commands.RequestEmailChange;
using Cobranca.Cidada.Application.Modules.Auth.Commands.RequestPasswordReset;
using Cobranca.Cidada.Application.Modules.Auth.Commands.RequestRegistrationCode;
using Cobranca.Cidada.Application.Modules.Auth.Queries.GetProfile;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.CrossCutting.IoC.Infrastructure.Identity;
using Cobranca.Cidada.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Cobranca.Cidada.Presentation.Controllers.V1;

[Route("api/v{version:apiVersion}/authentication")]
[ApiVersion("1.0")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public AuthenticationController(IMediator mediator, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _mediator = mediator;
        _environment = environment;
        _configuration = configuration;
    }

    /// <summary>Envia um código de 6 dígitos ao e-mail para iniciar o cadastro. A resposta não revela se o e-mail já tem conta</summary>
    [HttpPost("request-registration-code")]
    [AllowAnonymous]
    [EnableRateLimiting("email-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RequestRegistrationCode([FromBody] RequestRegistrationCodeRequest request)
        => (await _mediator.Send(new RequestRegistrationCodeCommand(request))).ToActionResult(this);

    /// <summary>Cria a conta (exige o código do e-mail e o aceite da política de privacidade) e inicia a sessão</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-anonymous")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        => CompleteAuthentication(await _mediator.Send(new RegisterCommand(request, HttpContext.GetSessionMetadata())), clearCookieOnError: false);

    /// <summary>Entra com e-mail e senha</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-anonymous")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
        => CompleteAuthentication(await _mediator.Send(new LoginCommand(request, HttpContext.GetSessionMetadata())), clearCookieOnError: false);

    /// <summary>
    /// Entra com o Google (ID token do botão "Entrar com o Google"). Se já existe conta com o mesmo e-mail, a conta Google é
    /// vinculada e entra no mesmo usuário; se não existe, cria a conta (exige o aceite da política de privacidade)
    /// </summary>
    [HttpPost("google")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-anonymous")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        => CompleteAuthentication(await _mediator.Send(new GoogleLoginCommand(request, HttpContext.GetSessionMetadata())), clearCookieOnError: false);

    /// <summary>Renova a sessão pelo cookie HttpOnly e rotaciona o refresh token</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-anonymous")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshSession()
    {
        if (!HasAllowedOrigin())
            return StatusCode(StatusCodes.Status403Forbidden);

        if (!Request.Cookies.TryGetValue(AuthSessionHttpExtensions.RefreshTokenCookieName, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { code = "Auth.MissingSession", description = "Sessão não encontrada." });

        return CompleteAuthentication(await _mediator.Send(new RefreshSessionCommand(refreshToken, HttpContext.GetSessionMetadata())));
    }

    /// <summary>Encerra a sessão vinculada ao cookie atual</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        if (!HasAllowedOrigin())
            return StatusCode(StatusCodes.Status403Forbidden);

        Request.Cookies.TryGetValue(AuthSessionHttpExtensions.RefreshTokenCookieName, out var refreshToken);
        var result = await _mediator.Send(new LogoutCommand(refreshToken));
        Response.DeleteRefreshTokenCookie(_environment);
        return result.ToActionResult(this);
    }

    /// <summary>Envia um código de 6 dígitos ao e-mail da conta para redefinir a senha. A resposta é a mesma exista ou não a conta</summary>
    [HttpPost("request-password-reset")]
    [AllowAnonymous]
    [EnableRateLimiting("email-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
        => (await _mediator.Send(new RequestPasswordResetCommand(request))).ToActionResult(this);

    /// <summary>Define a nova senha com o código recebido. Encerra todas as sessões da conta</summary>
    [HttpPost("confirm-password-reset")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-anonymous")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetRequest request)
    {
        var result = await _mediator.Send(new ConfirmPasswordResetCommand(request));
        if (!result.IsError)
            Response.DeleteRefreshTokenCookie(_environment);
        return result.ToActionResult(this);
    }

    /// <summary>Pede a troca do e-mail da conta: confere a senha e envia um código ao novo endereço</summary>
    [HttpPost("request-email-change")]
    [Authorize(Policy = IdentityDependencyInjection.AccountPolicy)]
    [EnableRateLimiting("email-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RequestEmailChange([FromBody] RequestEmailChangeRequest request)
        => (await _mediator.Send(new RequestEmailChangeCommand(HttpContext.GetUserId()!, request))).ToActionResult(this);

    /// <summary>Conclui a troca de e-mail com o código. Os votos acompanham o novo e-mail e é preciso entrar de novo</summary>
    [HttpPost("confirm-email-change")]
    [Authorize(Policy = IdentityDependencyInjection.AccountPolicy)]
    [EnableRateLimiting("auth-anonymous")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ConfirmEmailChange([FromBody] ConfirmEmailChangeRequest request)
    {
        var result = await _mediator.Send(new ConfirmEmailChangeCommand(HttpContext.GetUserId()!, request));
        if (!result.IsError)
            Response.DeleteRefreshTokenCookie(_environment);
        return result.ToActionResult(this);
    }

    /// <summary>Troca a senha da conta logada: confere a senha atual e encerra todas as sessões, exigindo novo login</summary>
    [HttpPost("change-password")]
    [Authorize(Policy = IdentityDependencyInjection.AccountPolicy)]
    [EnableRateLimiting("auth-anonymous")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await _mediator.Send(new ChangePasswordCommand(HttpContext.GetUserId()!, request));
        if (!result.IsError)
            Response.DeleteRefreshTokenCookie(_environment);
        return result.ToActionResult(this);
    }

    /// <summary>Retorna os dados da própria conta</summary>
    [HttpGet("me")]
    [Authorize(Policy = IdentityDependencyInjection.AccountPolicy)]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile()
        => (await _mediator.Send(new GetProfileQuery(HttpContext.GetUserId()!))).ToActionResult(this);

    /// <summary>Exclui a própria conta, as sessões e todos os votos ligados ao e-mail (autoatendimento LGPD)</summary>
    [HttpDelete("me")]
    [Authorize(Policy = IdentityDependencyInjection.AccountPolicy)]
    [EnableRateLimiting("auth-anonymous")]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        var result = await _mediator.Send(new DeleteAccountCommand(HttpContext.GetUserId()!, request));
        if (!result.IsError)
            Response.DeleteRefreshTokenCookie(_environment);
        return result.ToActionResult(this);
    }

    private IActionResult CompleteAuthentication(ErrorOr<LoginResponse> result, bool clearCookieOnError = true)
    {
        if (result.IsError)
        {
            if (clearCookieOnError)
                Response.DeleteRefreshTokenCookie(_environment);
            return result.ToActionResult(this);
        }

        Response.SetRefreshTokenCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiresAtUtc, _environment);
        return Ok(result.Value);
    }

    /// <summary>Mitiga CSRF nas rotas que dependem do cookie: a origem, quando enviada, precisa estar na lista permitida.</summary>
    private bool HasAllowedOrigin()
    {
        var origin = Request.Headers.Origin.ToString();
        if (string.IsNullOrWhiteSpace(origin))
            return true;

        var allowedOrigins = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        return allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }
}
