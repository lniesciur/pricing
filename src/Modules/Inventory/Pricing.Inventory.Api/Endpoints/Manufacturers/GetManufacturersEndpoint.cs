using FastEndpoints;
using Pricing.Inventory.Application.UseCases.GetManufacturers;
using Pricing.Inventory.Contracts.Manufacturers;

namespace Pricing.Inventory.Api.Endpoints.Manufacturers;

public class GetManufacturersEndpoint(GetManufacturersUseCase useCase)
    : EndpointWithoutRequest<GetManufacturersResponse>
{
    public override void Configure()
    {
        Get("/inventory/manufacturers");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(ct);
        await Send.OkAsync(result.Value!, ct);
    }
}
