using ErrorOr;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cobranca.Cidada.Presentation.Extensions;

namespace Cobranca.Cidada.Presentation.Middlewares;

public class ErrorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public ErrorMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var description = _env.IsProduction()
            ? "An unexpected error occurred."
            : ex.InnerException?.Message ?? ex.Message;

        var error = Error.Failure(code: "InternalServerError", description: description);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(error, options));
    }
}
