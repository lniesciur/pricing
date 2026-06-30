using Pricing.Import.Contracts.DeviceImports;
using Pricing.Import.Domain.ImportJobs;
using Pricing.Shared.Domain;

namespace Pricing.Import.Application.UseCases.GetDeviceImport;

public sealed class GetDeviceImportUseCase(IImportJobRepository importJobRepository)
{
    public async Task<Result<GetDeviceImportResponse>> ExecuteAsync(
        Guid jobId,
        CancellationToken ct = default)
    {
        var job = await importJobRepository.FindByIdAsync(new ImportJobId(jobId), ct);
        if (job is null)
            return Result<GetDeviceImportResponse>.Fail($"Import job '{jobId}' not found.");

        var errors = job.Errors
            .Select(e => new ImportJobErrorDto(e.RowNumber, e.ErrorMessage, e.ErrorType.ToString()))
            .ToList();

        return Result<GetDeviceImportResponse>.Ok(new GetDeviceImportResponse(
            JobId: job.Id.Value,
            FileName: job.FileName,
            Status: job.Status,
            ImportType: job.ImportType,
            Added: job.Added,
            Skipped: job.Skipped,
            Updated: job.Updated,
            Deleted: job.Deleted,
            Errors: errors));
    }
}
