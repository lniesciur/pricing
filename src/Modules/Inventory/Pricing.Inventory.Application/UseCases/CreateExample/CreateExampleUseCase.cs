using Pricing.Inventory.Domain.Example;
using Pricing.Inventory.Contracts.Example;
using Pricing.Shared.Domain;

namespace Pricing.Inventory.Application.UseCases.CreateExample;

public class CreateExampleUseCase(IExampleRepository repository, IInventoryUnitOfWork unitOfWork)
{
    public async Task<Result<CreateExampleResponse>> ExecuteAsync(string name, CancellationToken ct)
    {
        var example = ExampleAggregate.Create(name);

        await repository.AddAsync(example, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<CreateExampleResponse>.Ok(new CreateExampleResponse(example.Id.Value, example.Name));
    }
}
