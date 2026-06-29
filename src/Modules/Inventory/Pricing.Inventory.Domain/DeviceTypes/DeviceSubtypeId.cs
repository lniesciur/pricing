namespace Pricing.Inventory.Domain.DeviceTypes;

public record DeviceSubtypeId(Guid Value)
{
    public static DeviceSubtypeId New() => new(Guid.NewGuid());
}
