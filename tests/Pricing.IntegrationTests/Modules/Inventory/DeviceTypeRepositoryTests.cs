using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pricing.IntegrationTests.Infrastructure;
using Pricing.Inventory.Domain.DeviceTypes;
using Pricing.Inventory.Infrastructure.Persistence;
using Pricing.Inventory.Infrastructure.Persistence.Repositories;

namespace Pricing.IntegrationTests.Modules.Inventory;

public class DeviceTypeRepositoryTests : IClassFixture<ApiFactory>, IAsyncDisposable
{
    private readonly ApiFactory _factory;
    private readonly IServiceScope _scope;
    private readonly InventoryDbContext _dbContext;
    private readonly DeviceTypeRepository _repository;
    private readonly List<Guid> _createdTypeIds = [];

    public DeviceTypeRepositoryTests(ApiFactory factory)
    {
        _factory = factory;
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        _repository = new DeviceTypeRepository(_dbContext);
    }

    [Fact]
    public async Task AddAsync_WhenEntityIsNew_PersistsToDatabase()
    {
        // Arrange
        var deviceType = DeviceType.Create("LAPTOP-ADD", "Laptop");
        _createdTypeIds.Add(deviceType.Id.Value);

        // Act
        await _repository.AddAsync(deviceType);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Assert
        var persisted = await _dbContext.DeviceTypes.FindAsync(deviceType.Id);
        Assert.NotNull(persisted);
        Assert.Equal("LAPTOP-ADD", persisted.Code);
        Assert.Equal("Laptop", persisted.Name);
    }

    [Fact]
    public async Task AddAsync_WhenEntityHasSubtypes_PersistsSubtypesToDatabase()
    {
        // Arrange
        var deviceType = DeviceType.Create("PHONE-SUB", "Smartphone");
        deviceType.AddSubtype("ANDROID", "Android Phone");
        deviceType.AddSubtype("IOS", "iOS Phone");
        _createdTypeIds.Add(deviceType.Id.Value);

        // Act
        await _repository.AddAsync(deviceType);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Assert
        var persisted = await _dbContext.DeviceTypes
            .Include(t => t.Subtypes)
            .FirstOrDefaultAsync(t => t.Code == "PHONE-SUB");
        Assert.NotNull(persisted);
        Assert.Equal(2, persisted.Subtypes.Count);
        Assert.Contains(persisted.Subtypes, s => s.Code == "ANDROID" && s.Name == "Android Phone");
        Assert.Contains(persisted.Subtypes, s => s.Code == "IOS" && s.Name == "iOS Phone");
    }

    [Fact]
    public async Task FindByCodeAsync_WhenTypeExistsWithSubtypes_ReturnsTypeWithSubtypesEagerLoaded()
    {
        // Arrange
        var deviceType = DeviceType.Create("TABLET-FC", "Tablet");
        deviceType.AddSubtype("ANDROID-TAB", "Android Tablet");
        deviceType.AddSubtype("IPAD", "iPad");
        _createdTypeIds.Add(deviceType.Id.Value);

        await _repository.AddAsync(deviceType);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _repository.FindByCodeAsync("TABLET-FC");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TABLET-FC", result.Code);
        Assert.Equal("Tablet", result.Name);
        Assert.Equal(2, result.Subtypes.Count);
        Assert.Contains(result.Subtypes, s => s.Code == "ANDROID-TAB");
        Assert.Contains(result.Subtypes, s => s.Code == "IPAD");
    }

    [Fact]
    public async Task FindByCodeAsync_WhenTypeNotFound_ReturnsNull()
    {
        // Act
        var result = await _repository.FindByCodeAsync("DOES-NOT-EXIST-XYZ");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FindByCodeAsync_WhenTypeExistsWithNoSubtypes_ReturnsTypeWithEmptySubtypesList()
    {
        // Arrange
        var deviceType = DeviceType.Create("DESKTOP-FC", "Desktop");
        _createdTypeIds.Add(deviceType.Id.Value);

        await _repository.AddAsync(deviceType);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _repository.FindByCodeAsync("DESKTOP-FC");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Subtypes);
    }

    [Fact]
    public async Task FindAllAsync_WhenMultipleTypesExist_ReturnsAllWithSubtypes()
    {
        // Arrange
        var type1 = DeviceType.Create("CAMERA-FA1", "Camera");
        type1.AddSubtype("DSLR", "DSLR Camera");
        var type2 = DeviceType.Create("PRINTER-FA2", "Printer");
        _createdTypeIds.Add(type1.Id.Value);
        _createdTypeIds.Add(type2.Id.Value);

        await _repository.AddAsync(type1);
        await _repository.AddAsync(type2);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var results = await _repository.FindAllAsync();

        // Assert
        Assert.Contains(results, t => t.Code == "CAMERA-FA1" && t.Subtypes.Count == 1);
        Assert.Contains(results, t => t.Code == "PRINTER-FA2" && t.Subtypes.Count == 0);
    }

    [Fact]
    public async Task ExistsByCodeAsync_WhenTypeExists_ReturnsTrue()
    {
        // Arrange
        var deviceType = DeviceType.Create("WEARABLE-EX", "Wearable");
        _createdTypeIds.Add(deviceType.Id.Value);

        await _repository.AddAsync(deviceType);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var exists = await _repository.ExistsByCodeAsync("WEARABLE-EX");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByCodeAsync_WhenTypeDoesNotExist_ReturnsFalse()
    {
        // Act
        var exists = await _repository.ExistsByCodeAsync("NONEXISTENT-CODE-9999");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task SaveChanges_WhenDeviceTypeDeleted_CascadeDeletesSubtypes()
    {
        // Arrange
        var deviceType = DeviceType.Create("MONITOR-CD", "Monitor");
        deviceType.AddSubtype("LCD", "LCD Monitor");
        deviceType.AddSubtype("OLED", "OLED Monitor");

        await _repository.AddAsync(deviceType);
        await _dbContext.SaveChangesAsync();

        var subtypeIds = deviceType.Subtypes.Select(s => s.Id.Value).ToList();
        _dbContext.ChangeTracker.Clear();

        // Act
        var toDelete = await _dbContext.DeviceTypes.FindAsync(deviceType.Id);
        _dbContext.DeviceTypes.Remove(toDelete!);
        await _dbContext.SaveChangesAsync();

        // Assert — subtypes must be cascade-deleted by EF (OnDelete: Cascade)
        var remainingSubtypes = await _dbContext.Set<DeviceSubtype>()
            .Where(s => subtypeIds.Contains(s.Id.Value))
            .ToListAsync();
        Assert.Empty(remainingSubtypes);
        // No entry added to _createdTypeIds — the type was already deleted above
    }

    [Fact]
    public async Task AddAsync_WhenDuplicateCode_ThrowsDbUpdateException()
    {
        // Arrange
        var type1 = DeviceType.Create("ROUTER-DUP", "Router One");
        _createdTypeIds.Add(type1.Id.Value);

        await _repository.AddAsync(type1);
        await _dbContext.SaveChangesAsync(); // first save succeeds

        var type2 = DeviceType.Create("ROUTER-DUP", "Router Two");

        // Act & Assert
        await _repository.AddAsync(type2);
        await Assert.ThrowsAsync<DbUpdateException>(() => _dbContext.SaveChangesAsync());
        // type2 was never committed — unique index rejected it
        // type1 remains in DB and will be cleaned up via _createdTypeIds
    }

    [Fact]
    public async Task FindByCodeAsync_WhenCodeMatchIsExact_DoesNotReturnPartialMatches()
    {
        // Arrange
        var deviceType = DeviceType.Create("SERVER", "Server");
        _createdTypeIds.Add(deviceType.Id.Value);

        await _repository.AddAsync(deviceType);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _repository.FindByCodeAsync("SERVERS");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_WhenSubtypeIdRoundTrips_StronglyTypedIdSurvivesDbRoundTrip()
    {
        // Arrange
        var deviceType = DeviceType.Create("NAS-RT", "NAS Device");
        deviceType.AddSubtype("HDD-NAS", "HDD-based NAS");
        _createdTypeIds.Add(deviceType.Id.Value);

        var originalTypeId = deviceType.Id;
        var originalSubtypeId = deviceType.Subtypes[0].Id;

        // Act
        await _repository.AddAsync(deviceType);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Assert — strongly-typed IDs survive the round-trip through value converters
        var persisted = await _repository.FindByCodeAsync("NAS-RT");
        Assert.NotNull(persisted);
        Assert.Equal(originalTypeId, persisted.Id);
        Assert.Equal(originalSubtypeId, persisted.Subtypes[0].Id);
    }

    public async ValueTask DisposeAsync()
    {
        if (_createdTypeIds.Count > 0)
        {
            // Use a fresh scope to avoid operating on a potentially faulted DbContext
            await using var cleanupScope = _factory.Services.CreateAsyncScope();
            var cleanupDb = cleanupScope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var toDelete = await cleanupDb.DeviceTypes
                .Where(t => _createdTypeIds.Contains(t.Id.Value))
                .ToListAsync();

            if (toDelete.Count > 0)
            {
                cleanupDb.DeviceTypes.RemoveRange(toDelete);
                await cleanupDb.SaveChangesAsync();
            }
        }

        _scope.Dispose();
    }
}
