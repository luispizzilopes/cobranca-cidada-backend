using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Cobranca.Cidada.Presentation.Extensions;

public static class RateLimitExtension
{
    public static IServiceCollection AddRateLimitConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        int Limit(string key, int fallback) => int.TryParse(configuration[$"RateLimits:{key}"], out var value) && value > 0 ? value : fallback;

        services.AddRateLimiter(options =>
        {
            options.AddPolicy("public-read", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = Limit("PublicRead", 120),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy("auth-anonymous", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(15),
                        PermitLimit = Limit("AuthAnonymous", 30),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy("email-code", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(15),
                        PermitLimit = Limit("EmailCode", 10),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy("seo", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = Limit("Seo", 6000),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy("vote", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.User.FindFirst("vk")?.Value
                        ?? context.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = Limit("Vote", 60),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy("comment", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.User.FindFirst("vk")?.Value
                        ?? context.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = Limit("Comment", 10),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy("admin", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.User.FindFirst("Id")?.Value
                        ?? context.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = Limit("Admin", 120),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                var retryAfter = ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var duration)
                    ? Math.Ceiling(duration.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : "60";
                ctx.HttpContext.Response.Headers.RetryAfter = retryAfter;
                await ctx.HttpContext.Response.WriteAsJsonAsync(
                    new { code = "RateLimit.Ip", description = "Muitas requisições do mesmo IP. Tente novamente em instantes." },
                    ct);
            };
        });

        return services;
    }
}
