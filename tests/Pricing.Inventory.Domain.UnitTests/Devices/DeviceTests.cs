using Pricing.Inventory.Domain.Devices;

namespace Pricing.Inventory.Domain.UnitTests.Devices;

public class DeviceTests
{
    private const string EanCode = "5901234123457";
    private const string Name = "iPhone 15 Pro 256GB Black";
    private const string TypeCode = "SMARTPHONE";
    private const string SubtypeCode = "IPHONE";
    private const string ManufacturerCode = "APPLE";

    // -------------------------------------------------------------------------
    // Create — required properties
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_WhenCalled_SetsEanCodeProperty()
    {
        // Act
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode);

        // Assert
        Assert.Equal(EanCode, device.EanCode);
    }

    [Fact]
    public void Create_WhenCalled_SetsNameProperty()
    {
        // Act
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode);

        // Assert
        Assert.Equal(Name, device.Name);
    }

    [Fact]
    public void Create_WhenCalled_SetsTypeCodeProperty()
    {
        // Act
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode);

        // Assert
        Assert.Equal(TypeCode, device.TypeCode);
    }

    [Fact]
    public void Create_WhenCalled_SetsSubtypeCodeProperty()
    {
        // Act
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode);

        // Assert
        Assert.Equal(SubtypeCode, device.SubtypeCode);
    }

    [Fact]
    public void Create_WhenCalled_SetsManufacturerCodeProperty()
    {
        // Act
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode);

        // Assert
        Assert.Equal(ManufacturerCode, device.ManufacturerCode);
    }

    // -------------------------------------------------------------------------
    // Create — nullable optional properties
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_WhenSubtypeCodeIsNull_SubtypeCodePropertyIsNull()
    {
        // Act
        var device = Device.Create(EanCode, Name, TypeCode, null, ManufacturerCode);

        // Assert
        Assert.Null(device.SubtypeCode);
    }

    [Fact]
    public void Create_WhenManufacturerCodeIsNull_ManufacturerCodePropertyIsNull()
    {
        // Act
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, null);

        // Assert
        Assert.Null(device.ManufacturerCode);
    }

    [Fact]
    public void Create_WhenBothOptionalCodesAreNull_DeviceIsCreatedSuccessfully()
    {
        // Act
        var device = Device.Create(EanCode, Name, TypeCode, null, null);

        // Assert
        Assert.NotNull(device);
        Assert.Null(device.SubtypeCode);
        Assert.Null(device.ManufacturerCode);
    }

    // -------------------------------------------------------------------------
    // Create — identity
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_WhenCalled_AssignsNonEmptyId()
    {
        // Act
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode);

        // Assert
        Assert.NotNull(device.Id);
        Assert.NotEqual(Guid.Empty, device.Id.Value);
    }

    [Fact]
    public void Create_CalledTwice_ProducesDifferentIds()
    {
        // Act
        var first = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode);
        var second = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode);

        // Assert
        Assert.NotEqual(first.Id, second.Id);
    }

    // -------------------------------------------------------------------------
    // Property immutability
    // -------------------------------------------------------------------------

    [Fact]
    public void EanCode_PropertyHasNoPublicSetter()
    {
        // Arrange
        var property = typeof(Device).GetProperty(nameof(Device.EanCode));

        // Assert
        Assert.Null(property!.GetSetMethod());
    }

    [Fact]
    public void TypeCode_PropertyHasNoPublicSetter()
    {
        // Arrange
        var property = typeof(Device).GetProperty(nameof(Device.TypeCode));

        // Assert
        Assert.Null(property!.GetSetMethod());
    }
}
