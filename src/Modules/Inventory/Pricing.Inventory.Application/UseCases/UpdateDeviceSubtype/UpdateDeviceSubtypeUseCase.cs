using Pricing.Inventory.Contracts.DeviceTypes;
using Pricing.Inventory.Domain.DeviceTypes;
using Pricing.Shared.Domain;

namespace Pricing.Inventory.Application.UseCases.UpdateDeviceSubtype;

public sealed class UpdateDeviceSubtypeUseCase(IDeviceTypeRepository repository, IInventoryUnitOfWork unitOfWork)
{
    public async Task<Result<UpdateDeviceSubtypeResponse>> ExecuteAsync(string typeCode, string subtypeCode, string name, CancellationToken ct)
    {
        var deviceType = await repository.FindByCodeAsync(typeCode, ct);
        if (deviceType is null)
            return Result<UpdateDeviceSubtypeResponse>.Fail($"Device type with code '{typeCode}' not found.");

        var result = deviceType.UpdateSubtypeName(subtypeCode, name);
        if (result.IsFailure)
            return Result<UpdateDeviceSubtypeResponse>.Fail(result.Error!);

        await unitOfWork.SaveChangesAsync(ct);

        var subtype = deviceType.Subtypes.First(s => s.Code == subtypeCode);
        return Result<UpdateDeviceSubtypeResponse>.Ok(new UpdateDeviceSubtypeResponse(subtype.Code, subtype.Name));
    }
}
