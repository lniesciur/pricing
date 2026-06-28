using Pricing.Shared.Domain;

namespace Pricing.Inventory.Domain.Example.Events;

public record ExampleCreated(ExampleId Id, string Name) : IDomainEvent;
