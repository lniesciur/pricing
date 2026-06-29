using Pricing.Inventory.Contracts.DeviceTypes;
using Pricing.Inventory.Domain.DeviceTypes;
using Pricing.Shared.Domain;

namespace Pricing.Inventory.Application.UseCases.CreateDeviceType;

public sealed class CreateDeviceTypeUseCase(IDeviceTypeRepository repository, IInventoryUnitOfWork unitOfWork)
{
    public async Task<Result<CreateDeviceTypeResponse>> ExecuteAsync(string code, string name, CancellationToken ct)
    {
        if (await repository.ExistsByCodeAsync(code, ct))
            return Result<CreateDeviceTypeResponse>.Fail($"Device type with code '{code}' already exists.");

        var deviceType = DeviceType.Create(code, name);
        await repository.AddAsync(deviceType, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<CreateDeviceTypeResponse>.Ok(new CreateDeviceTypeResponse(deviceType.Code, deviceType.Name));
    }
}
