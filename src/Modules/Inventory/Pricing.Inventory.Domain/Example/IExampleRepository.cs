namespace Pricing.Inventory.Domain.Example;

public interface IExampleRepository
{
    Task AddAsync(ExampleAggregate example, CancellationToken ct = default);
}
