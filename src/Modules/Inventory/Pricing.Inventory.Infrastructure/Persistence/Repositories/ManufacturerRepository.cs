using Microsoft.EntityFrameworkCore;
using Pricing.Inventory.Domain.Manufacturers;

namespace Pricing.Inventory.Infrastructure.Persistence.Repositories;

public class ManufacturerRepository(InventoryDbContext context) : IManufacturerRepository
{
    public async Task<Manufacturer?> FindByCodeAsync(string code, CancellationToken ct = default) =>
        await context.Manufacturers.FirstOrDefaultAsync(m => m.Code == code, ct);

    public async Task<IReadOnlyList<Manufacturer>> FindAllAsync(CancellationToken ct = default) =>
        await context.Manufacturers.ToListAsync(ct);

    public async Task AddAsync(Manufacturer manufacturer, CancellationToken ct = default) =>
        await context.Manufacturers.AddAsync(manufacturer, ct);

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default) =>
        await context.Manufacturers.AnyAsync(m => m.Code == code, ct);
}
