using Pricing.Inventory.Domain.Devices;

namespace Pricing.Inventory.Domain.UnitTests.Devices;

public class DeviceCreateAttributesTests
{
    private const string EanCode = "5901234123457";
    private const string Name = "iPhone 15 Pro 256GB Black";
    private const string TypeCode = "SMARTPHONE";
    private const string SubtypeCode = "IPHONE";
    private const string ManufacturerCode = "APPLE";

    #region Section 2 — Device.Create — Attribute behaviour (SPEC-007)

    [Fact]
    public void Create_WhenAttributesIsNull_AttributesPropertyIsEmpty()
    {
        // Act
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode);

        // Assert
        Assert.Empty(device.Attributes);
    }

    [Fact]
    public void Create_WhenAttributesIsEmptyList_AttributesPropertyIsEmpty()
    {
        // Arrange
        var attributes = new List<DeviceAttribute>();

        // Act
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode, attributes);

        // Assert
        Assert.Empty(device.Attributes);
    }

    [Fact]
    public void Create_WhenSingleAttributeProvided_ReturnsDeviceWithOneAttribute()
    {
        // Arrange
        var attributes = new List<DeviceAttribute> { new("Color", "Red") };

        // Act
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode, attributes);

        // Assert
        Assert.Single(device.Attributes);
        Assert.Equal("Color", device.Attributes[0].Name);
        Assert.Equal("Red", device.Attributes[0].Value);
    }

    [Fact]
    public void Create_WhenAttributesHaveUniqueNames_ReturnsDeviceWithAllAttributes()
    {
        // Arrange
        var attributes = new List<DeviceAttribute>
        {
            new("Color", "Black"),
            new("Storage", "256GB"),
            new("Connectivity", "5G")
        };

        // Act
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode, attributes);

        // Assert
        Assert.Equal(3, device.Attributes.Count);
        Assert.Equal("Color", device.Attributes[0].Name);
        Assert.Equal("Storage", device.Attributes[1].Name);
        Assert.Equal("Connectivity", device.Attributes[2].Name);
    }

    [Fact]
    public void Create_WhenAttributesHaveDuplicateNamesSameCase_ThrowsInvalidOperationException()
    {
        // Arrange
        var attributes = new List<DeviceAttribute>
        {
            new("Color", "Red"),
            new("Color", "Blue")
        };

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode, attributes));

        // Assert
        Assert.Equal("Duplicate attribute names: Color", ex.Message);
    }

    [Fact]
    public void Create_WhenAttributesHaveDuplicateNamesDifferentCase_ThrowsInvalidOperationException()
    {
        // Arrange
        var attributes = new List<DeviceAttribute>
        {
            new("color", "Red"),
            new("Color", "Blue")
        };

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode, attributes));

        // Assert
        Assert.Equal("Duplicate attribute names: color", ex.Message);
    }

    [Fact]
    public void Create_WhenAttributesHaveDuplicateNamesAllUpperCase_ThrowsInvalidOperationException()
    {
        // Arrange
        var attributes = new List<DeviceAttribute>
        {
            new("color", "Red"),
            new("COLOR", "Blue")
        };

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode, attributes));

        // Assert
        Assert.Equal("Duplicate attribute names: color", ex.Message);
    }

    [Fact]
    public void Create_WhenAttributesHaveMultipleDuplicateNamePairs_ExceptionMessageContainsAllDuplicateNames()
    {
        // Arrange
        var attributes = new List<DeviceAttribute>
        {
            new("color", "Red"),
            new("Color", "Blue"),
            new("size", "L"),
            new("Size", "XL")
        };

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode, attributes));

        // Assert
        Assert.Equal("Duplicate attribute names: color, size", ex.Message);
    }

    [Fact]
    public void Create_WhenOneAttributeNameIsDuplicatedAmongMultiple_ThrowsInvalidOperationException()
    {
        // Arrange
        var attributes = new List<DeviceAttribute>
        {
            new("Color", "Red"),
            new("Size", "256GB"),
            new("color", "Blue")
        };

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode, attributes));

        // Assert
        Assert.Equal("Duplicate attribute names: Color", ex.Message);
    }

    [Fact]
    public void Create_WhenAttributesProvided_AttributesPropertyPreservesInputOrder()
    {
        // Arrange
        var attributes = new List<DeviceAttribute>
        {
            new("Storage", "256GB"),
            new("Color", "Black"),
            new("Connectivity", "5G")
        };

        // Act
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode, attributes);

        // Assert
        Assert.Equal("Storage", device.Attributes[0].Name);
        Assert.Equal("Color", device.Attributes[1].Name);
        Assert.Equal("Connectivity", device.Attributes[2].Name);
    }

    [Fact]
    public void Create_WhenAttributesProvided_NoDomainEventsRaised()
    {
        // Arrange
        var attributes = new List<DeviceAttribute> { new("Color", "Black") };

        // Act
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode, attributes);
        var events = device.PopDomainEvents();

        // Assert
        Assert.Empty(events);
    }

    [Fact]
    public void Create_WhenNoAttributesProvided_NoDomainEventsRaised()
    {
        // Act
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode);
        var events = device.PopDomainEvents();

        // Assert
        Assert.Empty(events);
    }

    [Fact]
    public void Create_WhenAttributeHasEmptyStringName_ThrowsInvalidOperationException()
    {
        // Arrange
        var attributes = new List<DeviceAttribute>
        {
            new("", "Red"),
            new("", "Blue")
        };

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode, attributes));

        // Assert
        Assert.Equal("Attribute name must not be empty or whitespace.", ex.Message);
    }

    [Fact]
    public void Create_WhenAttributeHasWhitespaceOnlyName_ThrowsInvalidOperationException()
    {
        // Arrange
        var attributes = new List<DeviceAttribute> { new("   ", "Red") };

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode, attributes));

        // Assert
        Assert.Equal("Attribute name must not be empty or whitespace.", ex.Message);
    }

    [Fact]
    public void Create_WhenAttributeHasEmptyStringValue_DeviceIsCreated()
    {
        // Arrange
        var attributes = new List<DeviceAttribute> { new("Color", "") };

        // Act
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode, attributes);

        // Assert
        Assert.Single(device.Attributes);
        Assert.Equal("", device.Attributes[0].Value);
    }

    [Fact]
    public void Create_WhenPopDomainEventsCalledTwice_SecondCallReturnsEmptyList()
    {
        // Arrange
        var device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode);

        // Act
        device.PopDomainEvents();
        var secondResult = device.PopDomainEvents();

        // Assert
        Assert.Empty(secondResult);
    }

    #endregion

    #region Section 3 — Aggregate invariants (cross-method)

    [Fact]
    public void Create_WhenDuplicateAttributeNamesDetected_NoDeviceInstanceReturned()
    {
        // Arrange
        var attributes = new List<DeviceAttribute>
        {
            new("Color", "Red"),
            new("color", "Blue")
        };
        Device? device = null;

        // Act
        var ex = Record.Exception(
            () => device = Device.Create(EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode, attributes)
        );

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Null(device);
    }

    #endregion
}
