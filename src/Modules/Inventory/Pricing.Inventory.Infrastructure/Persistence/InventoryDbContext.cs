using Microsoft.EntityFrameworkCore;
using Pricing.Inventory.Domain.DeviceTypes;
using Pricing.Inventory.Domain.Example;

namespace Pricing.Inventory.Infrastructure.Persistence;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<ExampleAggregate> Examples => Set<ExampleAggregate>();
    public DbSet<DeviceType> DeviceTypes => Set<DeviceType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inventory");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
