using Pricing.Inventory.Domain.Devices;

namespace Pricing.Inventory.Domain.UnitTests.Devices;

public class DeviceAttributeTests
{
    #region Section 1 — DeviceAttribute (Value Object)

    [Fact]
    public void DeviceAttribute_WhenNameAndValueAreIdentical_InstancesAreEqual()
    {
        // Arrange
        var first = new DeviceAttribute("Color", "Red");
        var second = new DeviceAttribute("Color", "Red");

        // Act
        var areEqual = first == second;

        // Assert
        Assert.True(areEqual);
        Assert.True(first.Equals(second));
    }

    [Fact]
    public void DeviceAttribute_WhenNamesAreDifferent_InstancesAreNotEqual()
    {
        // Arrange
        var first = new DeviceAttribute("Color", "Red");
        var second = new DeviceAttribute("Size", "Red");

        // Act
        var areEqual = first == second;

        // Assert
        Assert.False(areEqual);
    }

    [Fact]
    public void DeviceAttribute_WhenValuesAreDifferent_InstancesAreNotEqual()
    {
        // Arrange
        var first = new DeviceAttribute("Color", "Red");
        var second = new DeviceAttribute("Color", "Blue");

        // Act
        var areEqual = first == second;

        // Assert
        Assert.False(areEqual);
    }

    [Fact]
    public void DeviceAttribute_WhenCreated_NamePropertyReturnsProvidedValue()
    {
        // Arrange
        var attr = new DeviceAttribute("Color", "Red");

        // Act
        var name = attr.Name;

        // Assert
        Assert.Equal("Color", name);
    }

    [Fact]
    public void DeviceAttribute_WhenCreated_ValuePropertyReturnsProvidedValue()
    {
        // Arrange
        var attr = new DeviceAttribute("Color", "Red");

        // Act
        var value = attr.Value;

        // Assert
        Assert.Equal("Red", value);
    }

    [Fact]
    public void DeviceAttribute_NameProperty_IsInitOnly()
    {
        var setter = typeof(DeviceAttribute).GetProperty(nameof(DeviceAttribute.Name))!.SetMethod;
        Assert.NotNull(setter);
        var isInitOnly = setter!.ReturnParameter
            .GetRequiredCustomModifiers()
            .Contains(typeof(System.Runtime.CompilerServices.IsExternalInit));
        Assert.True(isInitOnly);
    }

    [Fact]
    public void DeviceAttribute_ValueProperty_IsInitOnly()
    {
        var setter = typeof(DeviceAttribute).GetProperty(nameof(DeviceAttribute.Value))!.SetMethod;
        Assert.NotNull(setter);
        var isInitOnly = setter!.ReturnParameter
            .GetRequiredCustomModifiers()
            .Contains(typeof(System.Runtime.CompilerServices.IsExternalInit));
        Assert.True(isInitOnly);
    }

    #endregion
}
