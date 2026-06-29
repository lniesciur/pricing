using Pricing.Shared.Domain;

namespace Pricing.Inventory.Domain.DeviceTypes.Events;

public record DeviceTypeCreated(DeviceTypeId Id, string Code, string Name) : IDomainEvent;
