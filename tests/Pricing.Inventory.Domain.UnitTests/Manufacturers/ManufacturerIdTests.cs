using Pricing.Inventory.Domain.Manufacturers;

namespace Pricing.Inventory.Domain.UnitTests.Manufacturers;

public class ManufacturerIdTests
{
    // -------------------------------------------------------------------------
    // New
    // -------------------------------------------------------------------------

    [Fact]
    public void New_WhenCalled_ReturnsNonEmptyGuid()
    {
        // Act
        var id = ManufacturerId.New();

        // Assert
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void New_CalledTwice_ProducesDifferentIds()
    {
        // Act
        var first = ManufacturerId.New();
        var second = ManufacturerId.New();

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
        var first = new ManufacturerId(guid);
        var second = new ManufacturerId(guid);

        // Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void Equality_WhenTwoIdsHaveDifferentGuids_AreNotEqual()
    {
        // Act
        var first = new ManufacturerId(Guid.NewGuid());
        var second = new ManufacturerId(Guid.NewGuid());

        // Assert
        Assert.NotEqual(first, second);
    }
}
