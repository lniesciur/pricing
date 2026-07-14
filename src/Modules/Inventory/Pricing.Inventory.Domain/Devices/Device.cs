using Pricing.Shared.Domain;

namespace Pricing.Inventory.Domain.Devices;

public class Device : AggregateRoot<DeviceId>
{
    private readonly List<DeviceAttribute> _attributes = [];

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
    public IReadOnlyList<DeviceAttribute> Attributes => _attributes;

    public static Device Create(string eanCode, string name, string typeCode, string? subtypeCode, string? manufacturerCode, IReadOnlyList<DeviceAttribute>? attributes = null)
    {
        if (attributes is { Count: > 0 })
        {
            var emptyNames = attributes.Where(a => string.IsNullOrWhiteSpace(a.Name)).ToList();
            if (emptyNames.Count > 0)
                throw new InvalidOperationException("Attribute name must not be empty or whitespace.");

            var duplicates = attributes
                .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count > 0)
                throw new InvalidOperationException($"Duplicate attribute names: {string.Join(", ", duplicates)}");

            var device = new Device(DeviceId.New(), eanCode, name, typeCode, subtypeCode, manufacturerCode);
            device._attributes.AddRange(attributes);
            return device;
        }

        return new(DeviceId.New(), eanCode, name, typeCode, subtypeCode, manufacturerCode);
    }
}
