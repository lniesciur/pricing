using FastEndpoints;
using Pricing.Import.Application.UseCases.GetDeviceImport;
using Pricing.Import.Contracts.DeviceImports;

namespace Pricing.Import.Api.Endpoints.DeviceImports;

public class GetDeviceImportRequest
{
    public Guid JobId { get; set; }
}

public class GetDeviceImportEndpoint(GetDeviceImportUseCase useCase)
    : Endpoint<GetDeviceImportRequest, GetDeviceImportResponse>
{
    public override void Configure()
    {
        Get("/import/device-imports/{jobId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetDeviceImportRequest req, CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(req.JobId, ct);

        if (result.IsFailure)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result.Value!, ct);
    }
}
