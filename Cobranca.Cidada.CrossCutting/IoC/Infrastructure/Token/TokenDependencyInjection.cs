using Cobranca.Cidada.Application.Interfaces.Services;
using Cobranca.Cidada.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cobranca.Cidada.CrossCutting.IoC.Infrastructure.Token;

public static class TokenDependencyInjection
{
    public static void AddTokenDependencyInjection(this IServiceCollection services)
    {
        services.AddScoped<ITokenJwtService, TokenJwtService>();
    }
}
