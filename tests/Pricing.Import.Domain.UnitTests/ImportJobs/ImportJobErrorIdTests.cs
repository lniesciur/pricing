using Pricing.Import.Domain.ImportJobs;

namespace Pricing.Import.Domain.UnitTests.ImportJobs;

public class ImportJobErrorIdTests
{
    // -------------------------------------------------------------------------
    // New
    // -------------------------------------------------------------------------

    [Fact]
    public void New_WhenCalled_ReturnsNonEmptyGuid()
    {
        // Act
        var id = ImportJobErrorId.New();

        // Assert
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void New_CalledTwice_ProducesDifferentIds()
    {
        // Act
        var first = ImportJobErrorId.New();
        var second = ImportJobErrorId.New();

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
        var first = new ImportJobErrorId(guid);
        var second = new ImportJobErrorId(guid);

        // Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void Equality_WhenTwoIdsHaveDifferentGuids_AreNotEqual()
    {
        // Act
        var first = new ImportJobErrorId(Guid.NewGuid());
        var second = new ImportJobErrorId(Guid.NewGuid());

        // Assert
        Assert.NotEqual(first, second);
    }
}
