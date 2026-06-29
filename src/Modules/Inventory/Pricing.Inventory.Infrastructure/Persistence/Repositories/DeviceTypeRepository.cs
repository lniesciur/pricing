using Microsoft.EntityFrameworkCore;
using Pricing.Inventory.Domain.DeviceTypes;

namespace Pricing.Inventory.Infrastructure.Persistence.Repositories;

public class DeviceTypeRepository(InventoryDbContext context) : IDeviceTypeRepository
{
    public async Task<DeviceType?> FindByCodeAsync(string code, CancellationToken ct = default) =>
        await context.DeviceTypes
            .Include(t => t.Subtypes)
            .FirstOrDefaultAsync(t => t.Code == code, ct);

    public async Task<IReadOnlyList<DeviceType>> FindAllAsync(CancellationToken ct = default) =>
        await context.DeviceTypes
            .Include(t => t.Subtypes)
            .ToListAsync(ct);

    public async Task AddAsync(DeviceType deviceType, CancellationToken ct = default) =>
        await context.DeviceTypes.AddAsync(deviceType, ct);

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default) =>
        await context.DeviceTypes.AnyAsync(t => t.Code == code, ct);
}
