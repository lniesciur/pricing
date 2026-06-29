namespace Pricing.Inventory.Domain.Manufacturers;

public interface IManufacturerRepository
{
    Task<Manufacturer?> FindByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Manufacturer>> FindAllAsync(CancellationToken ct = default);
    Task AddAsync(Manufacturer manufacturer, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
}
