using Microsoft.EntityFrameworkCore;
using Pricing.Import.Domain.ImportJobs;

namespace Pricing.Import.Infrastructure.Persistence;

public class ImportDbContext(DbContextOptions<ImportDbContext> options) : DbContext(options)
{
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("import");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ImportDbContext).Assembly);
    }
}
