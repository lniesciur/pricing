using Pricing.Inventory.Contracts.Devices;
using Pricing.Inventory.Domain.DeviceTypes;
using Pricing.Inventory.Domain.Devices;
using Pricing.Inventory.Domain.Manufacturers;

namespace Pricing.Inventory.Application.UseCases.RegisterDevices;

public sealed class RegisterDevicesUseCase(
    IDeviceTypeRepository deviceTypeRepository,
    IManufacturerRepository manufacturerRepository)
{
    public async Task<DeviceValidationResult> ExecuteAsync(
        IReadOnlyList<RegisterDeviceRequest> requests,
        CancellationToken ct = default)
    {
        var allTypes = await deviceTypeRepository.FindAllAsync(ct);
        var allManufacturers = await manufacturerRepository.FindAllAsync(ct);

        var typeLookup = allTypes.ToDictionary(
            t => t.Code,
            t => t.Subtypes.Select(s => s.Code).ToHashSet(StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
        var manufacturerCodes = allManufacturers
            .Select(m => m.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var devices = new List<Device>();
        var errors = new List<RegisterDeviceError>();

        foreach (var request in requests)
        {
            if (!typeLookup.TryGetValue(request.TypeCode, out var subtypes))
            {
                errors.Add(new RegisterDeviceError(request.EanCode, $"TypeCode '{request.TypeCode}' not found."));
                continue;
            }

            if (request.SubtypeCode is not null && !subtypes.Contains(request.SubtypeCode))
            {
                errors.Add(new RegisterDeviceError(request.EanCode,
                    $"SubtypeCode '{request.SubtypeCode}' not found for TypeCode '{request.TypeCode}'."));
                continue;
            }

            if (request.ManufacturerCode is not null && !manufacturerCodes.Contains(request.ManufacturerCode))
            {
                errors.Add(new RegisterDeviceError(request.EanCode, $"ManufacturerCode '{request.ManufacturerCode}' not found."));
                continue;
            }

            devices.Add(Device.Create(request.EanCode, request.Name, request.TypeCode, request.SubtypeCode, request.ManufacturerCode));
        }

        return new DeviceValidationResult(devices, errors);
    }
}

public record DeviceValidationResult(
    IReadOnlyList<Device> ValidDevices,
    IReadOnlyList<RegisterDeviceError> Errors);
