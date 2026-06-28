using Pricing.Inventory.Application;
using Pricing.Shared.Application;
using Pricing.Shared.Domain;

namespace Pricing.Inventory.Infrastructure.Persistence;

public class InventoryUnitOfWork(InventoryDbContext context, IDomainEventDispatcher dispatcher) : IInventoryUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        var events = context.ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .SelectMany(e => e.PopDomainEvents())
            .ToList();

        await context.SaveChangesAsync(ct);

        foreach (var ev in events)
            await dispatcher.DispatchAsync(ev, ct);
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken ct = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        try
        {
            await operation();
            await SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
