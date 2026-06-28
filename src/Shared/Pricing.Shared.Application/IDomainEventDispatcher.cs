using Pricing.Shared.Domain;

namespace Pricing.Shared.Application;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}
