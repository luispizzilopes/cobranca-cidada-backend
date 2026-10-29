using Cobranca.Cidada.Presentation.Middlewares;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Cobranca.Cidada.Presentation.Extensions;

public static class RequestPipelineConfigurationExtension
{
    public static void AddRequestPipelineConfigurations(this WebApplication app)
    {
        var forwarded = BuildForwardedHeadersOptions(app.Configuration);
        if (forwarded is not null)
            app.UseForwardedHeaders(forwarded);
        else if (app.Environment.IsProduction())
            app.Logger.LogWarning("ForwardedHeaders desligado em Produção: atrás de proxy, todos os clientes terão o IP do proxy e os limites por IP valerão para todos juntos.");

        app.ConfigureSwagger();

        var allowedOrigins = app.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        app.UseCors(c => c
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());

        app.UseSecurityHeaders();
        app.UseHttpsRedirection();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<ErrorMiddleware>();
        app.MapControllers();
    }

    /// <summary>
    /// Opções do <c>X-Forwarded-For</c>, ou nulo se desligado. Ligado, exige a lista de proxies/redes confiáveis
    /// (<c>ForwardedHeaders:KnownProxies</c> e/ou <c>KnownNetworks</c> em CIDR): sem ela, qualquer cliente forjaria o
    /// cabeçalho e escaparia de todos os limites por IP. Só cabeçalhos vindos desses endereços são aceitos.
    /// </summary>
    public static ForwardedHeadersOptions? BuildForwardedHeadersOptions(IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("ForwardedHeaders:Enabled"))
            return null;

        var proxies = (configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
        var networks = (configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();

        if (proxies.Length == 0 && networks.Length == 0)
            throw new InvalidOperationException(
                "ForwardedHeaders:Enabled exige ForwardedHeaders:KnownProxies e/ou ForwardedHeaders:KnownNetworks (CIDR) com os proxies confiáveis.");

        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = Math.Max(1, configuration.GetValue("ForwardedHeaders:ForwardLimit", 1))
        };
        options.KnownProxies.Clear();
        options.KnownNetworks.Clear();

        foreach (var proxy in proxies)
        {
            if (!System.Net.IPAddress.TryParse(proxy.Trim(), out var address))
                throw new InvalidOperationException($"ForwardedHeaders:KnownProxies contém um IP inválido: '{proxy}'.");
            options.KnownProxies.Add(address);
        }

        foreach (var network in networks)
        {
            var parts = network.Trim().Split('/');
            if (parts.Length != 2 || !System.Net.IPAddress.TryParse(parts[0], out var prefix) || !int.TryParse(parts[1], out var length))
                throw new InvalidOperationException($"ForwardedHeaders:KnownNetworks contém um CIDR inválido: '{network}'.");
            options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(prefix, length));
        }

        return options;
    }

    private static void ConfigureSwagger(this WebApplication app)
    {
        if (app.Environment.IsProduction())
        {
            var username = app.Configuration["Swagger:Username"] ?? "swagger";
            var password = app.Configuration["Swagger:Password"]!;

            app.UseWhen(
                context => context.Request.Path.StartsWithSegments("/swagger"),
                branch => branch.UseMiddleware<SwaggerBasicAuthMiddleware>(username, password));
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
            options.DefaultModelsExpandDepth(-1);
            options.RoutePrefix = "swagger";
        });
    }

    private static void UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["Referrer-Policy"] = "no-referrer";
            headers["X-Frame-Options"] = "DENY";
            if (!context.Request.Path.StartsWithSegments("/swagger"))
                headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
            await next();
        });
    }
}
