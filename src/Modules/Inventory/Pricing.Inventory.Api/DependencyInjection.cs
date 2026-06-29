using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Inventory.Infrastructure;
using Pricing.Inventory.Infrastructure.Persistence;
using Pricing.Inventory.Infrastructure.Seeding;

namespace Pricing.Inventory.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInventoryInfrastructure(configuration);
        return services;
    }

    public static async Task StartInventoryModuleAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await db.Database.MigrateAsync();
        var seeder = scope.ServiceProvider.GetRequiredService<InventorySeeder>();
        await seeder.SeedAsync();
    }
}
