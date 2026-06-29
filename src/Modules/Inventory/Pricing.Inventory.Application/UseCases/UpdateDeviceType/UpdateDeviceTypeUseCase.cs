using Pricing.Inventory.Contracts.DeviceTypes;
using Pricing.Inventory.Domain.DeviceTypes;
using Pricing.Shared.Domain;

namespace Pricing.Inventory.Application.UseCases.UpdateDeviceType;

public sealed class UpdateDeviceTypeUseCase(IDeviceTypeRepository repository, IInventoryUnitOfWork unitOfWork)
{
    public async Task<Result<UpdateDeviceTypeResponse>> ExecuteAsync(string code, string name, CancellationToken ct)
    {
        var deviceType = await repository.FindByCodeAsync(code, ct);
        if (deviceType is null)
            return Result<UpdateDeviceTypeResponse>.Fail($"Device type with code '{code}' not found.");

        deviceType.UpdateName(name);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<UpdateDeviceTypeResponse>.Ok(new UpdateDeviceTypeResponse(deviceType.Code, deviceType.Name));
    }
}
