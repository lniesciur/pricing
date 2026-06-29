using Pricing.Shared.Domain;

namespace Pricing.Inventory.Domain.DeviceTypes;

public class DeviceType : AggregateRoot<DeviceTypeId>
{
    private readonly List<DeviceSubtype> _subtypes = [];

    private DeviceType(DeviceTypeId id, string code, string name) : base(id)
    {
        Code = code;
        Name = name;
    }

    public string Code { get; private set; }
    public string Name { get; private set; }
    public IReadOnlyList<DeviceSubtype> Subtypes => _subtypes.AsReadOnly();

    public static DeviceType Create(string code, string name) =>
        new(DeviceTypeId.New(), code, name);

    public Result UpdateName(string name)
    {
        Name = name;
        return Result.Ok();
    }

    public Result AddSubtype(string code, string name)
    {
        if (_subtypes.Any(s => s.Code == code))
            return Result.Fail($"Subtype with code '{code}' already exists in this type.");

        _subtypes.Add(DeviceSubtype.Create(code, name));
        return Result.Ok();
    }

    public Result UpdateSubtypeName(string subtypeCode, string name)
    {
        var subtype = _subtypes.FirstOrDefault(s => s.Code == subtypeCode);
        if (subtype is null)
            return Result.Fail($"Subtype with code '{subtypeCode}' not found.");

        subtype.UpdateName(name);
        return Result.Ok();
    }
}
