using Pricing.Shared.Contracts;

namespace Pricing.Import.Domain.ImportJobs;

public interface IImportJobRepository
{
    Task AddAsync(ImportJob importJob, CancellationToken ct = default);
    Task<ImportJob?> FindByIdAsync(ImportJobId id, CancellationToken ct = default);
    Task<(IReadOnlyList<ImportJob> Items, int TotalCount)> ListAsync(ImportJobStatus? status, int page, int pageSize, CancellationToken ct = default);
}
