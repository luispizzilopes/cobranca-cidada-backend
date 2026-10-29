using ErrorOr;
using Cobranca.Cidada.Application.Modules.Seo.Queries.GetSeoFragment;
using Cobranca.Cidada.Application.Modules.Seo.Queries.GetSitemap;
using Cobranca.Cidada.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Cobranca.Cidada.Presentation.Controllers.V1;

/// <summary>
/// Apoio ao SEO. O nginx do site chama <c>head</c> e <c>body</c> (include SSI) para pôr no HTML inicial o que um SPA
/// só teria depois de executar JavaScript: título, descrição, Open Graph e dados estruturados de cada compromisso.
/// </summary>
[Route("api/v{version:apiVersion}/seo")]
[ApiVersion("1.0")]
[ApiController]
[AllowAnonymous]
[EnableRateLimiting("seo")]
public class SeoController : ControllerBase
{
    private readonly IMediator _mediator;

    public SeoController(IMediator mediator) => _mediator = mediator;

    /// <summary>Etiquetas do &lt;head&gt; para a rota informada; vazio se a página usa as padrão</summary>
    /// <param name="path">Caminho da página no site (ex.: /compromissos/ECON-006)</param>
    [HttpGet("head")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/html")]
    public async Task<IActionResult> Head([FromQuery] string? path)
        => Html(await _mediator.Send(new GetSeoFragmentQuery(path, SeoFragmentPart.Head)));

    /// <summary>Resumo em HTML do conteúdo da página, para quem não executa JavaScript; vazio se não houver</summary>
    /// <param name="path">Caminho da página no site (ex.: /compromissos/ECON-006)</param>
    [HttpGet("body")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/html")]
    public async Task<IActionResult> Body([FromQuery] string? path)
        => Html(await _mediator.Send(new GetSeoFragmentQuery(path, SeoFragmentPart.Body)));

    private IActionResult Html(ErrorOr<string> result)
        => result.IsError ? result.ToActionResult(this) : Content(result.Value, "text/html; charset=utf-8");
}

/// <summary>Sitemap XML. Na raiz, sem versão: o nginx do site o publica em <c>/sitemap.xml</c>.</summary>
[Route("sitemap.xml")]
[ApiController]
[ApiVersionNeutral]
[AllowAnonymous]
[EnableRateLimiting("seo")]
public class SitemapController : ControllerBase
{
    private readonly IMediator _mediator;

    public SitemapController(IMediator mediator) => _mediator = mediator;

    /// <summary>Sitemap com as páginas estáticas e todos os compromissos publicados</summary>
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "application/xml")]
    public async Task<IActionResult> Get()
    {
        var result = await _mediator.Send(new GetSitemapQuery());
        return result.IsError ? result.ToActionResult(this) : Content(result.Value, "application/xml; charset=utf-8");
    }
}
