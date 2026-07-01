using Microsoft.EntityFrameworkCore;
using Pricing.Inventory.Domain.Devices;
using Pricing.Inventory.Domain.DeviceTypes;
using Pricing.Inventory.Domain.Manufacturers;

namespace Pricing.Inventory.Infrastructure.Persistence;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<DeviceType> DeviceTypes => Set<DeviceType>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<Device> Devices => Set<Device>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inventory");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
