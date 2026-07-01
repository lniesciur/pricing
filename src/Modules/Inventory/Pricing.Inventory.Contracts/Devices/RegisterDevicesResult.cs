namespace Pricing.Inventory.Contracts.Devices;

public record RegisterDevicesResult(
    int Added,
    int Skipped,
    IReadOnlyList<RegisterDeviceError> Errors);
