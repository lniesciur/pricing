using FastEndpoints;
using Pricing.Import.Application.UseCases.ListDeviceImports;
using Pricing.Import.Contracts.DeviceImports;
using Pricing.Shared.Contracts;

namespace Pricing.Import.Api.Endpoints.DeviceImports;

public class ListDeviceImportsRequest
{
    public ImportJobStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ListDeviceImportsEndpoint(ListDeviceImportsUseCase useCase)
    : Endpoint<ListDeviceImportsRequest, ListDeviceImportsResponse>
{
    public override void Configure()
    {
        Get("/import/device-imports");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ListDeviceImportsRequest req, CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(req.Status, req.Page, req.PageSize, ct);
        await Send.OkAsync(result.Value!, ct);
    }
}
