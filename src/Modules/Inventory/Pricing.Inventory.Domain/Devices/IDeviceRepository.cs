namespace Pricing.Inventory.Domain.Devices;

public interface IDeviceRepository
{
    Task<HashSet<string>> FindExistingEanCodesAsync(IReadOnlyList<string> eanCodes, CancellationToken ct);
    Task BulkInsertAsync(IReadOnlyList<Device> devices, CancellationToken ct);
}
