using Cobranca.Cidada.CrossCutting.IoC.Application;
using Cobranca.Cidada.CrossCutting.IoC.Infrastructure;
using Cobranca.Cidada.Presentation.Swagger;

namespace Cobranca.Cidada.Presentation.Extensions;

public static class ConfigurationExtension
{
    public static void AddConfigurations(
        this IServiceCollection services,
        ConfigurationManager configurationManager,
        IWebHostEnvironment hostingEnvironment)
    {
        ValidateProductionSwaggerPassword(configurationManager, hostingEnvironment);
        ValidateSecrets(configurationManager, hostingEnvironment);

        services.AddControllers();
        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddSwaggerConfiguration(configurationManager);

        services.AddRateLimitConfiguration(configurationManager);

        services.AddInfrastructureDependencyInjection(configurationManager, hostingEnvironment);
        services.AddApplicationDependencyInjection(configurationManager);

        services.AddCustomLogger(configurationManager);

        services.AddCors();
    }

    private static void ValidateSecrets(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
    {
        if (!hostingEnvironment.IsProduction())
            return;

        static bool IsPlaceholder(string? value, int minLength)
            => string.IsNullOrWhiteSpace(value) || value.Length < minLength || value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase);

        if (IsPlaceholder(configuration["Jwt:key"], 32))
            throw new InvalidOperationException("Jwt:key deve ser definido (mínimo de 32 caracteres) em Produção.");
        if (IsPlaceholder(configuration["Voting:EmailHmacKey"], 32))
            throw new InvalidOperationException("Voting:EmailHmacKey deve ser definido (mínimo de 32 caracteres) em Produção.");
        if (string.IsNullOrWhiteSpace(configuration["Seo:SiteUrl"]) || configuration["Seo:SiteUrl"]!.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Seo:SiteUrl deve ser o endereço público do site (ex.: https://cobrancacidada.example) em Produção.");
        if (string.IsNullOrWhiteSpace(configuration["BrevoSettings:ApiKey"]) && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("BREVO_API_KEY")))
            throw new InvalidOperationException("BrevoSettings:ApiKey deve ser definido em Produção.");
    }

    private static void ValidateProductionSwaggerPassword(
        IConfiguration configuration,
        IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsProduction() &&
            string.IsNullOrWhiteSpace(configuration["Swagger:Password"]))
        {
            throw new InvalidOperationException(
                "Swagger:Password must be configured when the application runs in Production.");
        }
    }
}
