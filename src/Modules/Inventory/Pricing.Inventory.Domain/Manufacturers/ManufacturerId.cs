namespace Pricing.Inventory.Domain.Manufacturers;

public record ManufacturerId(Guid Value)
{
    public static ManufacturerId New() => new(Guid.NewGuid());
}
