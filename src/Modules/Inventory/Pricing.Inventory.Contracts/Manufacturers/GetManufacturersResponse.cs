namespace Pricing.Inventory.Contracts.Manufacturers;

public record GetManufacturersResponse(IReadOnlyList<ManufacturerDto> Manufacturers);
