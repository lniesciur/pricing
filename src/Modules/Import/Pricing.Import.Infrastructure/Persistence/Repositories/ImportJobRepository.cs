using Microsoft.EntityFrameworkCore;
using Pricing.Import.Domain.ImportJobs;
using Pricing.Shared.Contracts;

namespace Pricing.Import.Infrastructure.Persistence.Repositories;

public class ImportJobRepository(ImportDbContext context) : IImportJobRepository
{
    public async Task AddAsync(ImportJob importJob, CancellationToken ct = default) =>
        await context.ImportJobs.AddAsync(importJob, ct);

    public async Task<ImportJob?> FindByIdAsync(ImportJobId id, CancellationToken ct = default) =>
        await context.ImportJobs
            .Include(j => j.Errors)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task<(IReadOnlyList<ImportJob> Items, int TotalCount)> ListAsync(
        ImportJobStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.ImportJobs.AsNoTracking();

        if (status.HasValue)
            query = query.Where(j => j.Status == status.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
