using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Import.Infrastructure;

namespace Pricing.Import.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddImportModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddImportInfrastructure(configuration);
        return services;
    }
}
