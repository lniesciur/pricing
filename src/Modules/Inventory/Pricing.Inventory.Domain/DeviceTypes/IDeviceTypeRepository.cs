namespace Pricing.Inventory.Domain.DeviceTypes;

public interface IDeviceTypeRepository
{
    Task<DeviceType?> FindByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<DeviceType>> FindAllAsync(CancellationToken ct = default);
    Task AddAsync(DeviceType deviceType, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
}
