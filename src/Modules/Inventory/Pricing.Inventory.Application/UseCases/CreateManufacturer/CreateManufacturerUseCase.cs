using Pricing.Inventory.Contracts.Manufacturers;
using Pricing.Inventory.Domain.Manufacturers;
using Pricing.Shared.Domain;

namespace Pricing.Inventory.Application.UseCases.CreateManufacturer;

public sealed class CreateManufacturerUseCase(IManufacturerRepository repository, IInventoryUnitOfWork unitOfWork)
{
    public async Task<Result<CreateManufacturerResponse>> ExecuteAsync(string code, string name, CancellationToken ct)
    {
        if (await repository.ExistsByCodeAsync(code, ct))
            return Result<CreateManufacturerResponse>.Fail($"Manufacturer with code '{code}' already exists.");

        var manufacturer = Manufacturer.Create(code, name);
        await repository.AddAsync(manufacturer, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<CreateManufacturerResponse>.Ok(new CreateManufacturerResponse(manufacturer.Code, manufacturer.Name));
    }
}
