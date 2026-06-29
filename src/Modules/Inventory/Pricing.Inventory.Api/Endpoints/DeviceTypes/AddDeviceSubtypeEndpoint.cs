using FastEndpoints;
using Pricing.Inventory.Application.UseCases.AddDeviceSubtype;
using Pricing.Inventory.Contracts.DeviceTypes;

namespace Pricing.Inventory.Api.Endpoints.DeviceTypes;

public class AddDeviceSubtypeEndpoint(AddDeviceSubtypeUseCase useCase)
    : Endpoint<AddDeviceSubtypeRequest, AddDeviceSubtypeResponse>
{
    public override void Configure()
    {
        Post("/inventory/device-types/{code}/subtypes");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddDeviceSubtypeRequest req, CancellationToken ct)
    {
        var typeCode = Route<string>("code")!;
        var result = await useCase.ExecuteAsync(typeCode, req.Code, req.Name, ct);

        if (result.IsFailure)
        {
            AddError(result.Error!);
            await Send.ErrorsAsync(409, ct);
            return;
        }

        await Send.CreatedAtAsync<AddDeviceSubtypeEndpoint>(null, result.Value!, cancellation: ct);
    }
}
