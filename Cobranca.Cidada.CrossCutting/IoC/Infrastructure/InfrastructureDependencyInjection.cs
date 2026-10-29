using Cobranca.Cidada.CrossCutting.IoC.Infrastructure.Context;
using Cobranca.Cidada.CrossCutting.IoC.Infrastructure.Identity;
using Cobranca.Cidada.CrossCutting.IoC.Infrastructure.Repositories;
using Cobranca.Cidada.CrossCutting.IoC.Infrastructure.Services;
using Cobranca.Cidada.CrossCutting.IoC.Infrastructure.Token;
using Cobranca.Cidada.CrossCutting.IoC.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cobranca.Cidada.CrossCutting.IoC.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static void AddInfrastructureDependencyInjection(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddAppDbContextDependencyInjection(configuration);
        services.AddIdentityDependencyInjection(configuration);
        services.AddTokenDependencyInjection();
        services.AddRepositoriesDependencyInjection();
        services.AddUnitOfWorkDependencyInjection();
        services.AddServicesDependencyInjection(configuration, environment);
    }
}
