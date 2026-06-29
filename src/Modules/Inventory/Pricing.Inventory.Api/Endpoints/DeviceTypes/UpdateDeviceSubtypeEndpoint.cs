using FastEndpoints;
using Pricing.Inventory.Application.UseCases.UpdateDeviceSubtype;
using Pricing.Inventory.Contracts.DeviceTypes;

namespace Pricing.Inventory.Api.Endpoints.DeviceTypes;

public class UpdateDeviceSubtypeEndpoint(UpdateDeviceSubtypeUseCase useCase)
    : Endpoint<UpdateDeviceSubtypeRequest, UpdateDeviceSubtypeResponse>
{
    public override void Configure()
    {
        Patch("/inventory/device-types/{code}/subtypes/{subtypeCode}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateDeviceSubtypeRequest req, CancellationToken ct)
    {
        var typeCode = Route<string>("code")!;
        var subtypeCode = Route<string>("subtypeCode")!;
        var result = await useCase.ExecuteAsync(typeCode, subtypeCode, req.Name, ct);

        if (result.IsFailure)
        {
            AddError(result.Error!);
            await Send.ErrorsAsync(409, ct);
            return;
        }

        await Send.OkAsync(result.Value!, ct);
    }
}
