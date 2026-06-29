using Pricing.Inventory.Domain.Manufacturers;

namespace Pricing.Inventory.Domain.UnitTests.Manufacturers;

public class ManufacturerTests
{
    private const string ManufacturerCode = "APPLE";
    private const string ManufacturerName = "Apple Inc.";

    // -------------------------------------------------------------------------
    // Create
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_WhenValidCodeAndName_SetsCodeProperty()
    {
        // Act
        var manufacturer = Manufacturer.Create(ManufacturerCode, ManufacturerName);

        // Assert
        Assert.Equal(ManufacturerCode, manufacturer.Code);
    }

    [Fact]
    public void Create_WhenValidCodeAndName_SetsNameProperty()
    {
        // Act
        var manufacturer = Manufacturer.Create(ManufacturerCode, ManufacturerName);

        // Assert
        Assert.Equal(ManufacturerName, manufacturer.Name);
    }

    [Fact]
    public void Create_WhenValidCodeAndName_AssignsNonEmptyId()
    {
        // Act
        var manufacturer = Manufacturer.Create(ManufacturerCode, ManufacturerName);

        // Assert
        Assert.NotNull(manufacturer.Id);
        Assert.NotEqual(Guid.Empty, manufacturer.Id.Value);
    }

    [Fact]
    public void Create_CalledTwice_ProducesDifferentIds()
    {
        // Act
        var first = Manufacturer.Create(ManufacturerCode, ManufacturerName);
        var second = Manufacturer.Create(ManufacturerCode, ManufacturerName);

        // Assert
        Assert.NotEqual(first.Id, second.Id);
    }

    // -------------------------------------------------------------------------
    // Code immutability
    // -------------------------------------------------------------------------

    [Fact]
    public void Code_PropertyHasNoPublicSetter()
    {
        // Arrange
        var property = typeof(Manufacturer).GetProperty(nameof(Manufacturer.Code));

        // Assert — GetSetMethod() returns null when there is no public setter
        Assert.Null(property!.GetSetMethod());
    }

    // -------------------------------------------------------------------------
    // UpdateName
    // -------------------------------------------------------------------------

    [Fact]
    public void UpdateName_WhenCalled_ReturnsOkResult()
    {
        // Arrange
        var manufacturer = Manufacturer.Create(ManufacturerCode, ManufacturerName);

        // Act
        var result = manufacturer.UpdateName("Apple");

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void UpdateName_WhenCalled_ChangesNameToNewValue()
    {
        // Arrange
        const string updatedName = "Apple Corporation";
        var manufacturer = Manufacturer.Create(ManufacturerCode, ManufacturerName);

        // Act
        manufacturer.UpdateName(updatedName);

        // Assert
        Assert.Equal(updatedName, manufacturer.Name);
    }

    [Fact]
    public void UpdateName_WhenCalled_DoesNotAlterCodeProperty()
    {
        // Arrange
        var manufacturer = Manufacturer.Create(ManufacturerCode, ManufacturerName);

        // Act
        manufacturer.UpdateName("Apple Corporation");

        // Assert
        Assert.Equal(ManufacturerCode, manufacturer.Code);
    }

    [Theory]
    [InlineData("Samsung")]
    [InlineData("Sony Mobile Communications")]
    [InlineData("A")]
    public void UpdateName_WithVariousValidNames_ChangesNameSuccessfully(string newName)
    {
        // Arrange
        var manufacturer = Manufacturer.Create(ManufacturerCode, ManufacturerName);

        // Act
        var result = manufacturer.UpdateName(newName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newName, manufacturer.Name);
    }

    [Fact]
    public void UpdateName_CalledMultipleTimes_ReflectsLastName()
    {
        // Arrange
        const string finalName = "Apple Ltd.";
        var manufacturer = Manufacturer.Create(ManufacturerCode, ManufacturerName);

        // Act
        manufacturer.UpdateName("Intermediate Name");
        manufacturer.UpdateName(finalName);

        // Assert
        Assert.Equal(finalName, manufacturer.Name);
    }
}
