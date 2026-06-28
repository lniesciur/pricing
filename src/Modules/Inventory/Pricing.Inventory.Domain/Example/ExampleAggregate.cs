using Pricing.Inventory.Domain.Example.Events;
using Pricing.Shared.Domain;

namespace Pricing.Inventory.Domain.Example;

public class ExampleAggregate : AggregateRoot<ExampleId>
{
    private ExampleAggregate(ExampleId id, string name) : base(id)
    {
        Name = name;
    }

    public string Name { get; private set; }

    public static ExampleAggregate Create(string name)
    {
        var example = new ExampleAggregate(ExampleId.New(), name);
        example.RaiseDomainEvent(new ExampleCreated(example.Id, example.Name));
        return example;
    }
}
