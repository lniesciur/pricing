using Pricing.Inventory.Application.UseCases.RegisterDevices;
using Pricing.Inventory.Contracts.Devices;
using Pricing.Inventory.Domain.Devices;
using Pricing.Inventory.Facade;

namespace Pricing.Inventory.Infrastructure;

public class InventoryFacadeImpl(
    RegisterDevicesUseCase registerDevicesUseCase,
    IDeviceRepository deviceRepository) : IInventoryFacade
{
    public async Task<RegisterDevicesResult> RegisterDevicesAsync(
        IReadOnlyList<RegisterDeviceRequest> requests,
        CancellationToken ct = default)
    {
        var validation = await registerDevicesUseCase.ExecuteAsync(requests, ct);
        var validDevices = validation.ValidDevices;

        if (validDevices.Count == 0)
            return new RegisterDevicesResult(0, 0, validation.Errors);

        var existingEanCodes = await deviceRepository.FindExistingEanCodesAsync(
            validDevices.Select(d => d.EanCode).ToList(), ct);

        var newDevices = validDevices.Where(d => !existingEanCodes.Contains(d.EanCode)).ToList();
        var skippedCount = validDevices.Count - newDevices.Count;

        var allErrors = new List<RegisterDeviceError>(validation.Errors);
        foreach (var skipped in validDevices.Where(d => existingEanCodes.Contains(d.EanCode)))
            allErrors.Add(new RegisterDeviceError(skipped.EanCode, $"Device with EanCode '{skipped.EanCode}' already exists."));

        if (newDevices.Count > 0)
            await deviceRepository.BulkInsertAsync(newDevices, ct);

        return new RegisterDevicesResult(newDevices.Count, skippedCount, allErrors);
    }
}
