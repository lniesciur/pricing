using NSubstitute;
using Pricing.Inventory.Application.UseCases.UpdateDeviceType;
using Pricing.Inventory.Domain.DeviceTypes;

namespace Pricing.Inventory.Application.UnitTests.UseCases.UpdateDeviceType;

public sealed class UpdateDeviceTypeUseCaseTests
{
    private const string TypeCode = "SMARTPHONE";
    private const string OriginalName = "Smartphone";
    private const string UpdatedName = "Smartphones";

    private readonly IDeviceTypeRepository _repository = Substitute.For<IDeviceTypeRepository>();
    private readonly IInventoryUnitOfWork _unitOfWork = Substitute.For<IInventoryUnitOfWork>();
    private readonly UpdateDeviceTypeUseCase _sut;

    public UpdateDeviceTypeUseCaseTests()
    {
        _sut = new UpdateDeviceTypeUseCase(_repository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTypeNotFound_ReturnsFailure()
    {
        // Arrange
        _repository.FindByCodeAsync(TypeCode, Arg.Any<CancellationToken>()).Returns((DeviceType?)null);

        // Act
        var result = await _sut.ExecuteAsync(TypeCode, UpdatedName, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(TypeCode, result.Error);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTypeExists_UpdatesNameAndReturnsResponse()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, OriginalName);
        _repository.FindByCodeAsync(TypeCode, Arg.Any<CancellationToken>()).Returns(deviceType);

        // Act
        var result = await _sut.ExecuteAsync(TypeCode, UpdatedName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(TypeCode, result.Value!.Code);
        Assert.Equal(UpdatedName, result.Value!.Name);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTypeExists_CodeIsNotChangedByUpdate()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, OriginalName);
        _repository.FindByCodeAsync(TypeCode, Arg.Any<CancellationToken>()).Returns(deviceType);

        // Act
        var result = await _sut.ExecuteAsync(TypeCode, UpdatedName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(TypeCode, result.Value!.Code);
    }

    [Theory]
    [InlineData("NONEXISTENT_A")]
    [InlineData("NONEXISTENT_B")]
    public async Task ExecuteAsync_WhenTypeNotFound_ErrorMessageContainsCode(string missingCode)
    {
        // Arrange
        _repository.FindByCodeAsync(missingCode, Arg.Any<CancellationToken>()).Returns((DeviceType?)null);

        // Act
        var result = await _sut.ExecuteAsync(missingCode, UpdatedName, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(missingCode, result.Error);
    }
}
