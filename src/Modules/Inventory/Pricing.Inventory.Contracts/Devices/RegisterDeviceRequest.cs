namespace Pricing.Inventory.Contracts.Devices;

public record RegisterDeviceRequest(
    string EanCode,
    string Name,
    string TypeCode,
    string? SubtypeCode,
    string? ManufacturerCode);
