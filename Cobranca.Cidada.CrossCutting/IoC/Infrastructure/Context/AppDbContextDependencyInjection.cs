using Cobranca.Cidada.Infrastructure.Context;
using Cobranca.Cidada.Infrastructure.Interceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cobranca.Cidada.CrossCutting.IoC.Infrastructure.Context;

public static class AppDbContextDependencyInjection
{
    public static void AddAppDbContextDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<DomainEventsPublishInterceptor>();

        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var interceptor = serviceProvider.GetRequiredService<DomainEventsPublishInterceptor>();
            options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                .AddInterceptors(interceptor);
        });
    }
}
