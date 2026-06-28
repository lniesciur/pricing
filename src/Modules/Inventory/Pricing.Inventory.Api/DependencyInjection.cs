using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Inventory.Infrastructure;

namespace Pricing.Inventory.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInventoryInfrastructure(configuration);
        return services;
    }
}
