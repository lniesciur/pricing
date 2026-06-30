using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Import.Domain.ImportJobs;

namespace Pricing.Import.Infrastructure.Persistence.Configurations;

public class ImportJobConfiguration : IEntityTypeConfiguration<ImportJob>
{
    public void Configure(EntityTypeBuilder<ImportJob> builder)
    {
        builder.ToTable("ImportJobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .HasConversion(id => id.Value, value => new ImportJobId(value));

        builder.Property(j => j.FileName)
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(j => j.FileType)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(j => j.ImportType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(j => j.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(j => j.FileContent)
            .IsRequired();

        builder.Property(j => j.CreatedAt)
            .IsRequired();

        builder.HasMany(j => j.Errors)
            .WithOne()
            .HasForeignKey("ImportJobId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(j => j.Errors)
            .HasField("_errors")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
