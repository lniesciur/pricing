using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Inventory.Application;
using Pricing.Inventory.Facade;
using Pricing.Inventory.Infrastructure.DomainEvents;
using Pricing.Inventory.Infrastructure.Persistence;
using Pricing.Shared.Application;

namespace Pricing.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(o =>
            o.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IInventoryUnitOfWork, InventoryUnitOfWork>();
        services.AddScoped<IDomainEventDispatcher, NullDomainEventDispatcher>();
        services.AddScoped<IInventoryFacade, InventoryFacadeImpl>();

        services.Scan(scan => scan
            .FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Repository")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddInventoryApplication();

        return services;
    }
}
