using Pricing.Shared.Domain;

namespace Pricing.Inventory.Domain.DeviceTypes.Events;

public record DeviceSubtypeAdded(DeviceTypeId TypeId, DeviceSubtypeId SubtypeId, string Code, string Name) : IDomainEvent;
