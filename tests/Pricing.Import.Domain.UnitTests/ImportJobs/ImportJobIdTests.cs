using Pricing.Import.Domain.ImportJobs;

namespace Pricing.Import.Domain.UnitTests.ImportJobs;

public class ImportJobIdTests
{
    // -------------------------------------------------------------------------
    // New
    // -------------------------------------------------------------------------

    [Fact]
    public void New_WhenCalled_ReturnsNonEmptyGuid()
    {
        // Act
        var id = ImportJobId.New();

        // Assert
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void New_CalledTwice_ProducesDifferentIds()
    {
        // Act
        var first = ImportJobId.New();
        var second = ImportJobId.New();

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
        var first = new ImportJobId(guid);
        var second = new ImportJobId(guid);

        // Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void Equality_WhenTwoIdsHaveDifferentGuids_AreNotEqual()
    {
        // Act
        var first = new ImportJobId(Guid.NewGuid());
        var second = new ImportJobId(Guid.NewGuid());

        // Assert
        Assert.NotEqual(first, second);
    }
}
