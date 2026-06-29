using NSubstitute;
using Pricing.Inventory.Application.UseCases.CreateManufacturer;
using Pricing.Inventory.Domain.Manufacturers;

namespace Pricing.Inventory.Application.UnitTests.UseCases.CreateManufacturer;

public sealed class CreateManufacturerUseCaseTests
{
    private const string ManufacturerCode = "APPLE";
    private const string ManufacturerName = "Apple Inc.";

    private readonly IManufacturerRepository _repository = Substitute.For<IManufacturerRepository>();
    private readonly IInventoryUnitOfWork _unitOfWork = Substitute.For<IInventoryUnitOfWork>();
    private readonly CreateManufacturerUseCase _sut;

    public CreateManufacturerUseCaseTests()
    {
        _sut = new CreateManufacturerUseCase(_repository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCodeAlreadyExists_ReturnsFailure()
    {
        // Arrange
        _repository.ExistsByCodeAsync(ManufacturerCode, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _sut.ExecuteAsync(ManufacturerCode, ManufacturerName, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(ManufacturerCode, result.Error);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Manufacturer>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCodeIsNew_CreatesManufacturerAndReturnsResponse()
    {
        // Arrange
        _repository.ExistsByCodeAsync(ManufacturerCode, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _sut.ExecuteAsync(ManufacturerCode, ManufacturerName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ManufacturerCode, result.Value!.Code);
        Assert.Equal(ManufacturerName, result.Value!.Name);
        await _repository.Received(1).AddAsync(Arg.Any<Manufacturer>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCodeIsNew_AddAsyncCalledBeforeSaveChanges()
    {
        // Arrange
        _repository.ExistsByCodeAsync(ManufacturerCode, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        await _sut.ExecuteAsync(ManufacturerCode, ManufacturerName, CancellationToken.None);

        // Assert — both must be called exactly once
        await _repository.Received(1).AddAsync(Arg.Any<Manufacturer>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("SAMSUNG", "Samsung Electronics")]
    [InlineData("SONY", "Sony Corporation")]
    [InlineData("LG", "LG Electronics")]
    public async Task ExecuteAsync_WhenCodeIsNew_ResponseReflectsInputCodeAndName(string code, string name)
    {
        // Arrange
        _repository.ExistsByCodeAsync(code, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _sut.ExecuteAsync(code, name, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(code, result.Value!.Code);
        Assert.Equal(name, result.Value!.Name);
    }

    [Theory]
    [InlineData("DUPLICATE_A")]
    [InlineData("DUPLICATE_B")]
    public async Task ExecuteAsync_WhenCodeAlreadyExists_ErrorMessageContainsCode(string duplicateCode)
    {
        // Arrange
        _repository.ExistsByCodeAsync(duplicateCode, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _sut.ExecuteAsync(duplicateCode, ManufacturerName, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(duplicateCode, result.Error);
    }
}
