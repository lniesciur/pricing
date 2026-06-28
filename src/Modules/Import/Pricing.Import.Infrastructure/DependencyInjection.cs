using Pricing.Import.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Pricing.Import.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddImportInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Scan(scan => scan
            .FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Repository")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddImportApplication();

        return services;
    }
}
