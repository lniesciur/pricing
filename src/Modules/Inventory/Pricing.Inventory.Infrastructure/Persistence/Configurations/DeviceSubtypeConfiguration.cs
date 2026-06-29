using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Inventory.Domain.DeviceTypes;

namespace Pricing.Inventory.Infrastructure.Persistence.Configurations;

public class DeviceSubtypeConfiguration : IEntityTypeConfiguration<DeviceSubtype>
{
    public void Configure(EntityTypeBuilder<DeviceSubtype> builder)
    {
        builder.ToTable("DeviceSubtypes");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new DeviceSubtypeId(value));

        builder.Property(s => s.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();
    }
}
