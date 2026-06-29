namespace Pricing.Inventory.Domain.DeviceTypes;

public record DeviceTypeId(Guid Value)
{
    public static DeviceTypeId New() => new(Guid.NewGuid());
}
