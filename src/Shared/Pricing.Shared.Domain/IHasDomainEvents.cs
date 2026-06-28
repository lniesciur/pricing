namespace Pricing.Shared.Domain;

public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> PopDomainEvents();
}
