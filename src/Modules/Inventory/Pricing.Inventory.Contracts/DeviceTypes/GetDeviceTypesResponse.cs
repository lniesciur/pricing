namespace Pricing.Inventory.Contracts.DeviceTypes;

public record GetDeviceTypesResponse(IReadOnlyList<DeviceTypeDto> Types);
