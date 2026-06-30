using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Inventory.Domain.Devices;

namespace Pricing.Inventory.Infrastructure.Persistence.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasConversion(id => id.Value, value => new DeviceId(value));

        builder.Property(d => d.EanCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(d => d.EanCode)
            .IsUnique();

        builder.Property(d => d.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.TypeCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.SubtypeCode)
            .HasMaxLength(50);

        builder.Property(d => d.ManufacturerCode)
            .HasMaxLength(50);
    }
}
