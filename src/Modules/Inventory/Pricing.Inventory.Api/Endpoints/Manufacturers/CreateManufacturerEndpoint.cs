using FastEndpoints;
using Pricing.Inventory.Application.UseCases.CreateManufacturer;
using Pricing.Inventory.Contracts.Manufacturers;

namespace Pricing.Inventory.Api.Endpoints.Manufacturers;

public class CreateManufacturerEndpoint(CreateManufacturerUseCase useCase)
    : Endpoint<CreateManufacturerRequest, CreateManufacturerResponse>
{
    public override void Configure()
    {
        Post("/inventory/manufacturers");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateManufacturerRequest req, CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(req.Code, req.Name, ct);

        if (result.IsFailure)
        {
            AddError(result.Error!);
            await Send.ErrorsAsync(409, ct);
            return;
        }

        await Send.CreatedAtAsync<CreateManufacturerEndpoint>(null, result.Value!, cancellation: ct);
    }
}
