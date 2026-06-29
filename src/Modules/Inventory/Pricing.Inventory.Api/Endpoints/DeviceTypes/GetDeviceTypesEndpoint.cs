using FastEndpoints;
using Pricing.Inventory.Application.UseCases.GetDeviceTypes;
using Pricing.Inventory.Contracts.DeviceTypes;

namespace Pricing.Inventory.Api.Endpoints.DeviceTypes;

public class GetDeviceTypesEndpoint(GetDeviceTypesUseCase useCase)
    : EndpointWithoutRequest<GetDeviceTypesResponse>
{
    public override void Configure()
    {
        Get("/inventory/device-types");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(ct);
        await Send.OkAsync(result.Value!, ct);
    }
}
