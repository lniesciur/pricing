using Pricing.Inventory.Domain.Devices;

namespace Pricing.Inventory.Domain.UnitTests.Devices;

public class DeviceIdTests
{
    // -------------------------------------------------------------------------
    // New
    // -------------------------------------------------------------------------

    [Fact]
    public void New_WhenCalled_ReturnsNonEmptyGuid()
    {
        // Act
        var id = DeviceId.New();

        // Assert
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void New_CalledTwice_ProducesDifferentIds()
    {
        // Act
        var first = DeviceId.New();
        var second = DeviceId.New();

        // Assert
        Assert.NotEqual(first, second);
    }

    // -------------------------------------------------------------------------
    // Equality (record semantics)
    // -------------------------------------------------------------------------

    [Fact]
    public void Equality_WhenTwoIdsHaveSameGuid_AreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var first = new DeviceId(guid);
        var second = new DeviceId(guid);

        // Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void Equality_WhenTwoIdsHaveDifferentGuids_AreNotEqual()
    {
        // Act
        var first = new DeviceId(Guid.NewGuid());
        var second = new DeviceId(Guid.NewGuid());

        // Assert
        Assert.NotEqual(first, second);
    }
}
