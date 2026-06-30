using Pricing.Shared.Domain;

namespace Pricing.Inventory.Domain.Devices;

public class Device : AggregateRoot<DeviceId>
{
    private Device(DeviceId id, string eanCode, string name, string typeCode, string? subtypeCode, string? manufacturerCode)
        : base(id)
    {
        EanCode = eanCode;
        Name = name;
        TypeCode = typeCode;
        SubtypeCode = subtypeCode;
        ManufacturerCode = manufacturerCode;
    }

    public string EanCode { get; private set; }
    public string Name { get; private set; }
    public string TypeCode { get; private set; }
    public string? SubtypeCode { get; private set; }
    public string? ManufacturerCode { get; private set; }

    public static Device Create(string eanCode, string name, string typeCode, string? subtypeCode, string? manufacturerCode) =>
        new(DeviceId.New(), eanCode, name, typeCode, subtypeCode, manufacturerCode);
}
