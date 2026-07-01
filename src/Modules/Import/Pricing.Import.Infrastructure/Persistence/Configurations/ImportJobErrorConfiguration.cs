using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Import.Domain.ImportJobs;

namespace Pricing.Import.Infrastructure.Persistence.Configurations;

public class ImportJobErrorConfiguration : IEntityTypeConfiguration<ImportJobError>
{
    public void Configure(EntityTypeBuilder<ImportJobError> builder)
    {
        builder.ToTable("ImportJobErrors");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => new ImportJobErrorId(value));

        builder.Property(e => e.ErrorType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(1000)
            .IsRequired();
    }
}
