using Pricing.Shared.Domain;

namespace Pricing.Inventory.Domain.Manufacturers;

public class Manufacturer : AggregateRoot<ManufacturerId>
{
    private Manufacturer(ManufacturerId id, string code, string name) : base(id)
    {
        Code = code;
        Name = name;
    }

    public string Code { get; private set; }
    public string Name { get; private set; }

    public static Manufacturer Create(string code, string name) =>
        new(ManufacturerId.New(), code, name);

    public Result UpdateName(string name)
    {
        Name = name;
        return Result.Ok();
    }
}
