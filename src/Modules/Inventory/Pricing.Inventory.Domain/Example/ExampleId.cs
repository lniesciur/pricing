namespace Pricing.Inventory.Domain.Example;

public record ExampleId(Guid Value)
{
    public static ExampleId New() => new(Guid.NewGuid());
}
