using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace Cobranca.Cidada.CrossCutting.IoC.Infrastructure.UnitOfWork;

public static class UnitOfWorkDependencyInjection
{
    public static void AddUnitOfWorkDependencyInjection(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, Cidada.Infrastructure.UnitOfWork.UnitOfWork>();
    }
}
