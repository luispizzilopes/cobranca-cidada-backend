using System.Security.Cryptography;
using System.Text;

namespace Cobranca.Cidada.Presentation.Middlewares;

public sealed class SwaggerBasicAuthMiddleware
{
    private const string BasicScheme = "Basic ";

    private readonly RequestDelegate _next;
    private readonly byte[] _expectedUsernameHash;
    private readonly byte[] _expectedPasswordHash;

    public SwaggerBasicAuthMiddleware(RequestDelegate next, string username, string password)
    {
        _next = next;
        _expectedUsernameHash = Hash(username);
        _expectedPasswordHash = Hash(password);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (HasValidCredentials(context.Request.Headers.Authorization))
        {
            await _next(context);
            return;
        }

        context.Response.Headers.WWWAuthenticate = "Basic realm=\"Swagger\", charset=\"UTF-8\"";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }

    private bool HasValidCredentials(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith(BasicScheme, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            var encodedCredentials = authorizationHeader[BasicScheme.Length..].Trim();
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var separatorIndex = credentials.IndexOf(':');

            if (separatorIndex < 0)
            {
                return false;
            }

            var usernameHash = Hash(credentials[..separatorIndex]);
            var passwordHash = Hash(credentials[(separatorIndex + 1)..]);

            return CryptographicOperations.FixedTimeEquals(usernameHash, _expectedUsernameHash) &
                   CryptographicOperations.FixedTimeEquals(passwordHash, _expectedPasswordHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] Hash(string value) => SHA256.HashData(Encoding.UTF8.GetBytes(value));
}
