using FastEndpoints;
using Pricing.Inventory.Application.UseCases.UpdateDeviceType;
using Pricing.Inventory.Contracts.DeviceTypes;

namespace Pricing.Inventory.Api.Endpoints.DeviceTypes;

public class UpdateDeviceTypeEndpoint(UpdateDeviceTypeUseCase useCase)
    : Endpoint<UpdateDeviceTypeRequest, UpdateDeviceTypeResponse>
{
    public override void Configure()
    {
        Patch("/inventory/device-types/{code}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateDeviceTypeRequest req, CancellationToken ct)
    {
        var code = Route<string>("code")!;
        var result = await useCase.ExecuteAsync(code, req.Name, ct);

        if (result.IsFailure)
        {
            AddError(result.Error!);
            await Send.ErrorsAsync(409, ct);
            return;
        }

        await Send.OkAsync(result.Value!, ct);
    }
}
