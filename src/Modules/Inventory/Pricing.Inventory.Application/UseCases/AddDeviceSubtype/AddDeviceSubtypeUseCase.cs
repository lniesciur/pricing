using Pricing.Inventory.Contracts.DeviceTypes;
using Pricing.Inventory.Domain.DeviceTypes;
using Pricing.Shared.Domain;

namespace Pricing.Inventory.Application.UseCases.AddDeviceSubtype;

public sealed class AddDeviceSubtypeUseCase(IDeviceTypeRepository repository, IInventoryUnitOfWork unitOfWork)
{
    public async Task<Result<AddDeviceSubtypeResponse>> ExecuteAsync(string typeCode, string subtypeCode, string name, CancellationToken ct)
    {
        var deviceType = await repository.FindByCodeAsync(typeCode, ct);
        if (deviceType is null)
            return Result<AddDeviceSubtypeResponse>.Fail($"Device type with code '{typeCode}' not found.");

        var result = deviceType.AddSubtype(subtypeCode, name);
        if (result.IsFailure)
            return Result<AddDeviceSubtypeResponse>.Fail(result.Error!);

        await unitOfWork.SaveChangesAsync(ct);

        var added = deviceType.Subtypes.First(s => s.Code == subtypeCode);
        return Result<AddDeviceSubtypeResponse>.Ok(new AddDeviceSubtypeResponse(added.Code, added.Name));
    }
}
