using Pricing.Inventory.Contracts.Devices;

namespace Pricing.Inventory.Facade;

public interface IInventoryFacade
{
    Task<RegisterDevicesResult> RegisterDevicesAsync(
        IReadOnlyList<RegisterDeviceRequest> requests,
        CancellationToken ct = default);
}
