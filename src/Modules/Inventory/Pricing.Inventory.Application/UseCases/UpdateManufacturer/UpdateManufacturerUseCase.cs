using Pricing.Inventory.Contracts.Manufacturers;
using Pricing.Inventory.Domain.Manufacturers;
using Pricing.Shared.Domain;

namespace Pricing.Inventory.Application.UseCases.UpdateManufacturer;

public sealed class UpdateManufacturerUseCase(IManufacturerRepository repository, IInventoryUnitOfWork unitOfWork)
{
    public async Task<Result<UpdateManufacturerResponse>> ExecuteAsync(string code, string name, CancellationToken ct)
    {
        var manufacturer = await repository.FindByCodeAsync(code, ct);
        if (manufacturer is null)
            return Result<UpdateManufacturerResponse>.Fail($"Manufacturer with code '{code}' not found.");

        manufacturer.UpdateName(name);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<UpdateManufacturerResponse>.Ok(new UpdateManufacturerResponse(manufacturer.Code, manufacturer.Name));
    }
}
