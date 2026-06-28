using Pricing.Inventory.Domain.Example;

namespace Pricing.Inventory.Infrastructure.Persistence.Repositories;

public class ExampleRepository(InventoryDbContext context) : IExampleRepository
{
    public async Task AddAsync(ExampleAggregate example, CancellationToken ct = default) =>
        await context.Examples.AddAsync(example, ct);
}
