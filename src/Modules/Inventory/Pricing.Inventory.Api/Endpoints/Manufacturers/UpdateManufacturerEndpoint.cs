using FastEndpoints;
using Pricing.Inventory.Application.UseCases.UpdateManufacturer;
using Pricing.Inventory.Contracts.Manufacturers;

namespace Pricing.Inventory.Api.Endpoints.Manufacturers;

public class UpdateManufacturerEndpoint(UpdateManufacturerUseCase useCase)
    : Endpoint<UpdateManufacturerRequest, UpdateManufacturerResponse>
{
    public override void Configure()
    {
        Patch("/inventory/manufacturers/{code}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateManufacturerRequest req, CancellationToken ct)
    {
        var code = Route<string>("code")!;
        var result = await useCase.ExecuteAsync(code, req.Name, ct);

        if (result.IsFailure)
        {
            AddError(result.Error!);
            await Send.ErrorsAsync(404, ct);
            return;
        }

        await Send.OkAsync(result.Value!, ct);
    }
}
