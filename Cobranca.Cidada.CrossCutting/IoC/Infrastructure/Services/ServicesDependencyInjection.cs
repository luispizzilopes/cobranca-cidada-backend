using Cobranca.Cidada.Domain.Interfaces.Caching;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Infrastructure.Services;
using Cobranca.Cidada.Infrastructure.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Cobranca.Cidada.CrossCutting.IoC.Infrastructure.Services;

public static class ServicesDependencyInjection
{
    public static void AddServicesDependencyInjection(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        AddCache(services, configuration);
        AddBrevoEmailService(services, configuration, environment);
        services.AddSingleton<IVoterKeyService, VoterKeyService>();
        services.AddSingleton<IVerificationCodeStore, VerificationCodeStore>();
        services.AddSingleton<IGoogleIdentityVerifier, GoogleIdentityVerifier>();
    }

    private static void AddCache(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddSingleton<ICacheService, NoOpCacheService>();
            services.AddDistributedMemoryCache();
            services.AddSingleton<IVoteActivityStore, MemoryVoteActivityStore>();
            return;
        }

        services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(redisConnection);
            options.AllowAdmin = true;
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IVoteActivityStore, RedisVoteActivityStore>();
    }

    private static void AddBrevoEmailService(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var section = configuration.GetSection("BrevoSettings");
        var settings = new BrevoSettings
        {
            ApiKey = string.IsNullOrWhiteSpace(section["ApiKey"]) ? Environment.GetEnvironmentVariable("BREVO_API_KEY") ?? string.Empty : section["ApiKey"]!,
            FromEmail = string.IsNullOrWhiteSpace(section["FromEmail"]) ? Environment.GetEnvironmentVariable("EMAIL_FROM_EMAIL") ?? string.Empty : section["FromEmail"]!,
            FromName = string.IsNullOrWhiteSpace(section["FromName"]) ? Environment.GetEnvironmentVariable("EMAIL_FROM_NAME") ?? string.Empty : section["FromName"]!,
        };

        services.Configure<BrevoSettings>(options =>
        {
            options.ApiKey = settings.ApiKey;
            options.FromEmail = settings.FromEmail;
            options.FromName = settings.FromName;
        });

        services.AddHttpClient("Brevo", client =>
        {
            client.BaseAddress = new Uri("https://api.brevo.com/v3/");
            client.DefaultRequestHeaders.Add("api-key", settings.ApiKey);
        });

        if (string.IsNullOrWhiteSpace(settings.ApiKey) && !environment.IsProduction())
            services.AddScoped<IEmailService, ConsoleEmailService>();
        else
            services.AddScoped<IEmailService, BrevoEmailService>();
    }
}
