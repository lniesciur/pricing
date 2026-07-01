using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Pricing.Import.Application.UseCases.UploadDeviceImport;
using Pricing.Import.Contracts.DeviceImports;

namespace Pricing.Import.Api.Endpoints.DeviceImports;

public class UploadDeviceImportRequest
{
    public IFormFile File { get; set; } = null!;
}

public class UploadDeviceImportEndpoint(UploadDeviceImportUseCase useCase)
    : Endpoint<UploadDeviceImportRequest, UploadDeviceImportResponse>
{
    public override void Configure()
    {
        Post("/import/device-imports");
        AllowAnonymous();
        AllowFileUploads();
    }

    public override async Task HandleAsync(UploadDeviceImportRequest req, CancellationToken ct)
    {
        await using var stream = req.File.OpenReadStream();
        var result = await useCase.ExecuteAsync(req.File.FileName, stream, ct);
        
        if (result.IsFailure)
        {
            AddError(result.Error!);
            await Send.ErrorsAsync(422, ct);
            return;
        }

        await Send.AcceptedAtAsync<GetDeviceImportEndpoint>(
            new { jobId = result.Value!.JobId },
            result.Value!,
            cancellation: ct);
    }
}
