using Pricing.Inventory.Domain.DeviceTypes;

namespace Pricing.Inventory.Domain.UnitTests.DeviceTypes;

public class DeviceTypeIdTests
{
    [Fact]
    public void New_WhenCalled_ReturnsIdWithNonEmptyGuid()
    {
        // Act
        var id = DeviceTypeId.New();

        // Assert
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void New_WhenCalledTwice_ReturnsDistinctIds()
    {
        // Act
        var first = DeviceTypeId.New();
        var second = DeviceTypeId.New();

        // Assert
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void DeviceTypeId_WhenTwoInstancesHaveSameGuid_AreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var first = new DeviceTypeId(guid);
        var second = new DeviceTypeId(guid);

        // Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void DeviceTypeId_WhenTwoInstancesHaveDifferentGuids_AreNotEqual()
    {
        // Act
        var first = new DeviceTypeId(Guid.NewGuid());
        var second = new DeviceTypeId(Guid.NewGuid());

        // Assert
        Assert.NotEqual(first, second);
    }
}
