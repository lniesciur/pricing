using Pricing.Inventory.Contracts.DeviceTypes;
using Pricing.Inventory.Domain.DeviceTypes;
using Pricing.Shared.Domain;

namespace Pricing.Inventory.Application.UseCases.GetDeviceTypes;

public sealed class GetDeviceTypesUseCase(IDeviceTypeRepository repository)
{
    public async Task<Result<GetDeviceTypesResponse>> ExecuteAsync(CancellationToken ct)
    {
        var types = await repository.FindAllAsync(ct);

        var dtos = types.Select(t => new DeviceTypeDto(
            t.Code,
            t.Name,
            t.Subtypes.Select(s => new DeviceSubtypeDto(s.Code, s.Name)).ToList()
        )).ToList();

        return Result<GetDeviceTypesResponse>.Ok(new GetDeviceTypesResponse(dtos));
    }
}
