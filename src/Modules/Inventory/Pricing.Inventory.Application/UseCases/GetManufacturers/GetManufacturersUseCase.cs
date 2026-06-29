using Pricing.Inventory.Contracts.Manufacturers;
using Pricing.Inventory.Domain.Manufacturers;
using Pricing.Shared.Domain;

namespace Pricing.Inventory.Application.UseCases.GetManufacturers;

public sealed class GetManufacturersUseCase(IManufacturerRepository repository)
{
    public async Task<Result<GetManufacturersResponse>> ExecuteAsync(CancellationToken ct)
    {
        var manufacturers = await repository.FindAllAsync(ct);

        var dtos = manufacturers
            .Select(m => new ManufacturerDto(m.Code, m.Name))
            .ToList();

        return Result<GetManufacturersResponse>.Ok(new GetManufacturersResponse(dtos));
    }
}
