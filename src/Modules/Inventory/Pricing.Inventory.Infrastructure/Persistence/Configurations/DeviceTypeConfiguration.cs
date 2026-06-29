using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Inventory.Domain.DeviceTypes;

namespace Pricing.Inventory.Infrastructure.Persistence.Configurations;

public class DeviceTypeConfiguration : IEntityTypeConfiguration<DeviceType>
{
    public void Configure(EntityTypeBuilder<DeviceType> builder)
    {
        builder.ToTable("DeviceTypes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => new DeviceTypeId(value));

        builder.Property(t => t.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(t => t.Code)
            .IsUnique();

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasMany(t => t.Subtypes)
            .WithOne()
            .HasForeignKey("TypeId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Subtypes)
            .HasField("_subtypes")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
