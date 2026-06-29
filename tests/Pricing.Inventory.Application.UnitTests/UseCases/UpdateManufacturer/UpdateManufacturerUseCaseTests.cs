using NSubstitute;
using Pricing.Inventory.Application.UseCases.UpdateManufacturer;
using Pricing.Inventory.Domain.Manufacturers;

namespace Pricing.Inventory.Application.UnitTests.UseCases.UpdateManufacturer;

public sealed class UpdateManufacturerUseCaseTests
{
    private const string ManufacturerCode = "APPLE";
    private const string OriginalName = "Apple Inc.";
    private const string UpdatedName = "Apple Incorporated";

    private readonly IManufacturerRepository _repository = Substitute.For<IManufacturerRepository>();
    private readonly IInventoryUnitOfWork _unitOfWork = Substitute.For<IInventoryUnitOfWork>();
    private readonly UpdateManufacturerUseCase _sut;

    public UpdateManufacturerUseCaseTests()
    {
        _sut = new UpdateManufacturerUseCase(_repository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WhenManufacturerNotFound_ReturnsFailure()
    {
        // Arrange
        _repository.FindByCodeAsync(ManufacturerCode, Arg.Any<CancellationToken>()).Returns((Manufacturer?)null);

        // Act
        var result = await _sut.ExecuteAsync(ManufacturerCode, UpdatedName, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(ManufacturerCode, result.Error);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenManufacturerExists_UpdatesNameAndReturnsResponse()
    {
        // Arrange
        var manufacturer = Manufacturer.Create(ManufacturerCode, OriginalName);
        _repository.FindByCodeAsync(ManufacturerCode, Arg.Any<CancellationToken>()).Returns(manufacturer);

        // Act
        var result = await _sut.ExecuteAsync(ManufacturerCode, UpdatedName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ManufacturerCode, result.Value!.Code);
        Assert.Equal(UpdatedName, result.Value!.Name);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenManufacturerExists_CodeIsNotChangedByUpdate()
    {
        // Arrange
        var manufacturer = Manufacturer.Create(ManufacturerCode, OriginalName);
        _repository.FindByCodeAsync(ManufacturerCode, Arg.Any<CancellationToken>()).Returns(manufacturer);

        // Act
        var result = await _sut.ExecuteAsync(ManufacturerCode, UpdatedName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ManufacturerCode, result.Value!.Code);
    }

    [Fact]
    public async Task ExecuteAsync_WhenManufacturerExists_ResponseReflectsNewName()
    {
        // Arrange
        var manufacturer = Manufacturer.Create(ManufacturerCode, OriginalName);
        _repository.FindByCodeAsync(ManufacturerCode, Arg.Any<CancellationToken>()).Returns(manufacturer);

        // Act
        var result = await _sut.ExecuteAsync(ManufacturerCode, UpdatedName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(OriginalName, result.Value!.Name);
        Assert.Equal(UpdatedName, result.Value!.Name);
    }

    [Theory]
    [InlineData("NONEXISTENT_A")]
    [InlineData("NONEXISTENT_B")]
    public async Task ExecuteAsync_WhenManufacturerNotFound_ErrorMessageContainsCode(string missingCode)
    {
        // Arrange
        _repository.FindByCodeAsync(missingCode, Arg.Any<CancellationToken>()).Returns((Manufacturer?)null);

        // Act
        var result = await _sut.ExecuteAsync(missingCode, UpdatedName, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(missingCode, result.Error);
    }

    [Theory]
    [InlineData("SAMSUNG", "Samsung Electronics", "Samsung Electronics Co.")]
    [InlineData("SONY", "Sony Corporation", "Sony Group Corporation")]
    public async Task ExecuteAsync_WhenManufacturerExists_ReturnsUpdatedNameForVariousInputs(
        string code, string originalName, string newName)
    {
        // Arrange
        var manufacturer = Manufacturer.Create(code, originalName);
        _repository.FindByCodeAsync(code, Arg.Any<CancellationToken>()).Returns(manufacturer);

        // Act
        var result = await _sut.ExecuteAsync(code, newName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(code, result.Value!.Code);
        Assert.Equal(newName, result.Value!.Name);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
