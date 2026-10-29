using Cobranca.Cidada.Application.Modules.Auth.Models;

namespace Cobranca.Cidada.Presentation.Extensions;

public static class AuthSessionHttpExtensions
{
    public const string RefreshTokenCookieName = "cobranca.refresh";
    public const string RefreshCookiePath = "/api/v1/authentication";
    private const int DeviceNameMaxLength = 160;

    public static SessionMetadata GetSessionMetadata(this HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        return new SessionMetadata(BuildDeviceName(userAgent), Truncate(userAgent, 512), Truncate(ipAddress, 64));
    }

    public static void SetRefreshTokenCookie(
        this HttpResponse response,
        string refreshToken,
        DateTime expiresAtUtc,
        IWebHostEnvironment environment)
    {
        response.Cookies.Append(RefreshTokenCookieName, refreshToken, CookieOptionsFor(environment, new DateTimeOffset(expiresAtUtc)));
    }

    public static void DeleteRefreshTokenCookie(this HttpResponse response, IWebHostEnvironment environment)
        => response.Cookies.Delete(RefreshTokenCookieName, CookieOptionsFor(environment, null));

    private static CookieOptions CookieOptionsFor(IWebHostEnvironment environment, DateTimeOffset? expires)
    {
        var local = environment.IsEnvironment("Localhost") || environment.IsEnvironment("Testing");
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !local,
            SameSite = local ? SameSiteMode.Lax : SameSiteMode.None,
            Expires = expires,
            Path = RefreshCookiePath,
            IsEssential = true
        };
    }

    private static string BuildDeviceName(string userAgent)
    {
        var browser = userAgent.Contains("Edg/", StringComparison.OrdinalIgnoreCase) ? "Edge" :
            userAgent.Contains("Firefox/", StringComparison.OrdinalIgnoreCase) ? "Firefox" :
            userAgent.Contains("Chrome/", StringComparison.OrdinalIgnoreCase) ? "Chrome" :
            userAgent.Contains("Safari/", StringComparison.OrdinalIgnoreCase) ? "Safari" : "Navegador";

        var system = userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ? "iPhone" :
            userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase) ? "iPad" :
            userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase) ? "Android" :
            userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase) ? "Windows" :
            userAgent.Contains("Macintosh", StringComparison.OrdinalIgnoreCase) ? "macOS" :
            userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase) ? "Linux" : "dispositivo desconhecido";

        var name = $"{browser} em {system}";
        return name[..Math.Min(name.Length, DeviceNameMaxLength)];
    }

    private static string? Truncate(string? value, int maxLength)
        => string.IsNullOrWhiteSpace(value) ? null : value[..Math.Min(value.Length, maxLength)];
}
