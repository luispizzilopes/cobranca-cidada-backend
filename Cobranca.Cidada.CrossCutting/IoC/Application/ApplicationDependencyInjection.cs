using Cobranca.Cidada.Application.Behaviors;
using Cobranca.Cidada.Application.Common;
using Cobranca.Cidada.Application.Modules.Auth.Services;
using Cobranca.Cidada.Application.Modules.Seo.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cobranca.Cidada.CrossCutting.IoC.Application;

public static class ApplicationDependencyInjection
{
    public static void AddApplicationDependencyInjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var applicationAssembly = typeof(ValidationBehavior<,>).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RetryOnConcurrencyBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        services.AddSingleton(TimeProvider.System);
        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddSingleton<VotePolicy>();
        services.AddSingleton<SeoSettings>();
        services.AddSingleton<VoteBurstSettings>();
        services.AddScoped<VoteBurstGuard>();
        services.AddScoped<ProfanityFilter>();
        services.AddScoped<AuthSessionService>();
        services.AddScoped<AccountReauthenticationService>();
        services.AddScoped<AdminCommitmentReader>();
        services.AddUseCasesDependencyInjection();
    }
}
