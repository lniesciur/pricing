using Pricing.Shared.Application;
using Pricing.Shared.Domain;

namespace Pricing.Inventory.Infrastructure.DomainEvents;

public class NullDomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default) =>
        Task.CompletedTask;
}
