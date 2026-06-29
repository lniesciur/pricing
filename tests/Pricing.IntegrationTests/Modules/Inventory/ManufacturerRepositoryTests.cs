using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pricing.IntegrationTests.Infrastructure;
using Pricing.Inventory.Domain.Manufacturers;
using Pricing.Inventory.Infrastructure.Persistence;
using Pricing.Inventory.Infrastructure.Persistence.Repositories;

namespace Pricing.IntegrationTests.Modules.Inventory;

public class ManufacturerRepositoryTests : IClassFixture<ApiFactory>, IAsyncDisposable
{
    private readonly ApiFactory _factory;
    private readonly IServiceScope _scope;
    private readonly InventoryDbContext _dbContext;
    private readonly ManufacturerRepository _repository;
    private readonly List<Guid> _createdIds = [];

    public ManufacturerRepositoryTests(ApiFactory factory)
    {
        _factory = factory;
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        _repository = new ManufacturerRepository(_dbContext);
    }

    [Fact]
    public async Task AddAsync_WhenEntityIsNew_PersistsToDatabase()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("MR-ADD-1", "Test Manufacturer Add");
        _createdIds.Add(manufacturer.Id.Value);

        // Act
        await _repository.AddAsync(manufacturer);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Assert
        var persisted = await _dbContext.Manufacturers.FindAsync(manufacturer.Id);
        Assert.NotNull(persisted);
        Assert.Equal("MR-ADD-1", persisted.Code);
        Assert.Equal("Test Manufacturer Add", persisted.Name);
    }

    [Fact]
    public async Task AddAsync_WhenEntityPersisted_StronglyTypedIdSurvivesDbRoundTrip()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("MR-RT-1", "Round Trip Manufacturer");
        _createdIds.Add(manufacturer.Id.Value);
        var originalId = manufacturer.Id;

        // Act
        await _repository.AddAsync(manufacturer);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Assert — ManufacturerId value converter must preserve the Guid round-trip
        var persisted = await _repository.FindByCodeAsync("MR-RT-1");
        Assert.NotNull(persisted);
        Assert.Equal(originalId, persisted.Id);
        Assert.IsType<ManufacturerId>(persisted.Id);
    }

    [Fact]
    public async Task AddAsync_WhenDuplicateCode_ThrowsDbUpdateException()
    {
        // Arrange
        var first = Manufacturer.Create("MR-DUP", "Duplicate First");
        _createdIds.Add(first.Id.Value);

        await _repository.AddAsync(first);
        await _dbContext.SaveChangesAsync(); // first save succeeds

        var second = Manufacturer.Create("MR-DUP", "Duplicate Second");

        // Act & Assert
        await _repository.AddAsync(second);
        await Assert.ThrowsAsync<DbUpdateException>(() => _dbContext.SaveChangesAsync());
        // second was never committed — unique index on Code rejected it
        // first remains in DB and will be cleaned up via _createdIds
    }

    [Fact]
    public async Task FindByCodeAsync_WhenManufacturerExists_ReturnsManufacturer()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("MR-FC-1", "Find By Code Manufacturer");
        _createdIds.Add(manufacturer.Id.Value);

        await _repository.AddAsync(manufacturer);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _repository.FindByCodeAsync("MR-FC-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MR-FC-1", result.Code);
        Assert.Equal("Find By Code Manufacturer", result.Name);
    }

    [Fact]
    public async Task FindByCodeAsync_WhenManufacturerNotFound_ReturnsNull()
    {
        // Act
        var result = await _repository.FindByCodeAsync("MR-DOES-NOT-EXIST-XYZ");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FindByCodeAsync_WhenCodeMatchIsExact_DoesNotReturnPartialMatches()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("MR-PARTIAL", "Partial Match Manufacturer");
        _createdIds.Add(manufacturer.Id.Value);

        await _repository.AddAsync(manufacturer);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act — searching for a prefix of the stored code must return null
        var result = await _repository.FindByCodeAsync("MR-PARTIA");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FindAllAsync_WhenCalled_ReturnsAllManufacturersIncludingSeedData()
    {
        // Arrange — seed migration inserts 20 well-known manufacturers (APPLE, SAMSUNG, etc.)
        // No extra data needed; we only assert the floor count.

        // Act
        var results = await _repository.FindAllAsync();

        // Assert
        Assert.True(results.Count >= 20,
            $"Expected at least 20 seed manufacturers but found {results.Count}.");
    }

    [Fact]
    public async Task FindAllAsync_WhenAdditionalManufacturerAdded_IncludesItInResults()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("MR-FA-1", "Find All Extra Manufacturer");
        _createdIds.Add(manufacturer.Id.Value);

        await _repository.AddAsync(manufacturer);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var results = await _repository.FindAllAsync();

        // Assert
        Assert.Contains(results, m => m.Code == "MR-FA-1" && m.Name == "Find All Extra Manufacturer");
    }

    [Fact]
    public async Task ExistsByCodeAsync_WhenManufacturerExists_ReturnsTrue()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("MR-EX-1", "Exists Manufacturer");
        _createdIds.Add(manufacturer.Id.Value);

        await _repository.AddAsync(manufacturer);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var exists = await _repository.ExistsByCodeAsync("MR-EX-1");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByCodeAsync_WhenManufacturerDoesNotExist_ReturnsFalse()
    {
        // Act
        var exists = await _repository.ExistsByCodeAsync("MR-NONEXISTENT-9999");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsByCodeAsync_WhenCodeMatchIsExact_DoesNotMatchSuperstring()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("MR-SONY", "Sony Test");
        _createdIds.Add(manufacturer.Id.Value);

        await _repository.AddAsync(manufacturer);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act — searching with a longer code that merely starts with the stored code
        var exists = await _repository.ExistsByCodeAsync("MR-SONY-EXTENDED");

        // Assert
        Assert.False(exists);
    }

    // TODO: add test for FindByCodeAsync case-sensitivity once the collation behaviour
    //       of the SQL Server instance (CI_AS vs CS_AS) is confirmed in a schema ADR

    public async ValueTask DisposeAsync()
    {
        if (_createdIds.Count > 0)
        {
            // Use a fresh scope to avoid operating on a potentially faulted DbContext
            await using var cleanupScope = _factory.Services.CreateAsyncScope();
            var cleanupDb = cleanupScope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var toDelete = await cleanupDb.Manufacturers
                .Where(m => _createdIds.Contains(m.Id.Value))
                .ToListAsync();

            if (toDelete.Count > 0)
            {
                cleanupDb.Manufacturers.RemoveRange(toDelete);
                await cleanupDb.SaveChangesAsync();
            }
        }

        _scope.Dispose();
    }
}
