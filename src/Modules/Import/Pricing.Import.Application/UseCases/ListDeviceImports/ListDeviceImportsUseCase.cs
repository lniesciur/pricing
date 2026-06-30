using Pricing.Import.Contracts.DeviceImports;
using Pricing.Import.Domain.ImportJobs;
using Pricing.Shared.Contracts;
using Pricing.Shared.Domain;

namespace Pricing.Import.Application.UseCases.ListDeviceImports;

public sealed class ListDeviceImportsUseCase(IImportJobRepository importJobRepository)
{
    public async Task<Result<ListDeviceImportsResponse>> ExecuteAsync(
        ImportJobStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await importJobRepository.ListAsync(status, page, pageSize, ct);

        var dtos = items
            .Select(j => new DeviceImportSummaryDto(
                JobId: j.Id.Value,
                FileName: j.FileName,
                Status: j.Status.ToString(),
                Added: j.Added,
                Skipped: j.Skipped,
                Updated: j.Updated,
                Deleted: j.Deleted,
                CreatedAt: j.CreatedAt))
            .ToList();

        return Result<ListDeviceImportsResponse>.Ok(new ListDeviceImportsResponse(dtos, totalCount, page, pageSize));
    }
}
