namespace Pricing.Inventory.Domain.Devices;

public record DeviceId(Guid Value)
{
    public static DeviceId New() => new(Guid.NewGuid());
}
