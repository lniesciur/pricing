using Pricing.Import.Contracts.DeviceImports;
using Pricing.Import.Domain.ImportJobs;
using Pricing.Shared.Contracts;
using Pricing.Shared.Domain;

namespace Pricing.Import.Application.UseCases.UploadDeviceImport;

public sealed class UploadDeviceImportUseCase(
    IImportJobRepository importJobRepository,
    IImportUnitOfWork unitOfWork,
    IImportJobScheduler scheduler)
{
    public async Task<Result<UploadDeviceImportResponse>> ExecuteAsync(
        string fileName,
        Stream fileStream,
        CancellationToken ct = default)
    {
        var fileType = ResolveFileType(fileName);
        if (fileType is null)
            return Result<UploadDeviceImportResponse>.Fail("Only .csv and .xlsx files are supported.");

        var content = new byte[fileStream.Length];
        _ = await fileStream.ReadAsync(content, ct);

        var job = ImportJob.Create(fileName, fileType.Value, ImportType.DeviceImport, content);

        await importJobRepository.AddAsync(job, ct);
        await unitOfWork.SaveChangesAsync(ct);

        scheduler.EnqueueDeviceImportProcessing(job.Id.Value);

        return Result<UploadDeviceImportResponse>.Ok(new UploadDeviceImportResponse(job.Id.Value));
    }

    private static FileType? ResolveFileType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".csv" => FileType.Csv,
            ".xlsx" => FileType.Xlsx,
            _ => null
        };
}
