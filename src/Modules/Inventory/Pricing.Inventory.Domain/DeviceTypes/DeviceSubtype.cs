using Pricing.Shared.Domain;

namespace Pricing.Inventory.Domain.DeviceTypes;

public class DeviceSubtype : Entity<DeviceSubtypeId>
{
    private DeviceSubtype(DeviceSubtypeId id, string code, string name) : base(id)
    {
        Code = code;
        Name = name;
    }

    public string Code { get; private set; }
    public string Name { get; private set; }

    internal static DeviceSubtype Create(string code, string name) =>
        new(DeviceSubtypeId.New(), code, name);

    internal void UpdateName(string name) => Name = name;
}
