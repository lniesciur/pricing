using Pricing.Inventory.Domain.DeviceTypes;

namespace Pricing.Inventory.Domain.UnitTests.DeviceTypes;

public class DeviceSubtypeIdTests
{
    [Fact]
    public void New_WhenCalled_ReturnsIdWithNonEmptyGuid()
    {
        // Act
        var id = DeviceSubtypeId.New();

        // Assert
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void New_WhenCalledTwice_ReturnsDistinctIds()
    {
        // Act
        var first = DeviceSubtypeId.New();
        var second = DeviceSubtypeId.New();

        // Assert
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void DeviceSubtypeId_WhenTwoInstancesHaveSameGuid_AreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var first = new DeviceSubtypeId(guid);
        var second = new DeviceSubtypeId(guid);

        // Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void DeviceSubtypeId_WhenTwoInstancesHaveDifferentGuids_AreNotEqual()
    {
        // Act
        var first = new DeviceSubtypeId(Guid.NewGuid());
        var second = new DeviceSubtypeId(Guid.NewGuid());

        // Assert
        Assert.NotEqual(first, second);
    }
}
