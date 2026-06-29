using FastEndpoints;
using Pricing.Inventory.Application.UseCases.CreateDeviceType;
using Pricing.Inventory.Contracts.DeviceTypes;

namespace Pricing.Inventory.Api.Endpoints.DeviceTypes;

public class CreateDeviceTypeEndpoint(CreateDeviceTypeUseCase useCase)
    : Endpoint<CreateDeviceTypeRequest, CreateDeviceTypeResponse>
{
    public override void Configure()
    {
        Post("/inventory/device-types");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateDeviceTypeRequest req, CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(req.Code, req.Name, ct);

        if (result.IsFailure)
        {
            AddError(result.Error!);
            await Send.ErrorsAsync(409, ct);
            return;
        }

        await Send.CreatedAtAsync<CreateDeviceTypeEndpoint>(null, result.Value!, cancellation: ct);
    }
}
