using Pricing.Inventory.Domain.Example;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Pricing.Inventory.Infrastructure.Persistence.Configurations;

public class ExampleConfiguration : IEntityTypeConfiguration<ExampleAggregate>
{
    public void Configure(EntityTypeBuilder<ExampleAggregate> builder)
    {
        builder.ToTable("Examples");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => new ExampleId(value));

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();
    }
}
