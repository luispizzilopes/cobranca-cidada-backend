using Cobranca.Cidada.Application.Interfaces.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Cobranca.Cidada.CrossCutting.IoC.Application;

public static class UseCasesDependencyInjection
{
    public static void AddUseCasesDependencyInjection(this IServiceCollection services)
    {
        var assembly = typeof(IUseCase).Assembly;

        var useCaseTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IUseCase).IsAssignableFrom(t));

        foreach (var type in useCaseTypes)
            services.AddScoped(type);
    }
}
