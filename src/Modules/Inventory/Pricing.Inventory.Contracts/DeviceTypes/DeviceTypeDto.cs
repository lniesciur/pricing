namespace Pricing.Inventory.Contracts.DeviceTypes;

public record DeviceTypeDto(string Code, string Name, IReadOnlyList<DeviceSubtypeDto> Subtypes);
