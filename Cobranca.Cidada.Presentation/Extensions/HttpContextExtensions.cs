namespace Cobranca.Cidada.Presentation.Extensions;

public static class HttpContextExtensions
{
    /// <summary>Id do usuário. Só existe em tokens de conta.</summary>
    public static string? GetUserId(this HttpContext httpContext)
        => httpContext.User.FindFirst("Id")?.Value;

    /// <summary>Chave pseudônima do votante (HMAC do e-mail). Existe em tokens de conta e de votante.</summary>
    public static string? GetVoterKey(this HttpContext httpContext)
        => httpContext.User.FindFirst("vk")?.Value;

    /// <summary>Verdadeiro para quem votou só com e-mail verificado, sem conta.</summary>
    public static bool IsVoterOnly(this HttpContext httpContext)
        => httpContext.User.FindFirst("typ")?.Value == "voter";

    /// <summary>
    /// IP do cliente. Atrás de proxy, só é o IP real se <c>ForwardedHeaders:Enabled</c> estiver ligado
    /// (e o proxy for confiável); senão todos os clientes parecem ter o IP do proxy.
    /// </summary>
    public static string? GetClientIp(this HttpContext httpContext)
        => httpContext.Connection.RemoteIpAddress?.ToString();
}
