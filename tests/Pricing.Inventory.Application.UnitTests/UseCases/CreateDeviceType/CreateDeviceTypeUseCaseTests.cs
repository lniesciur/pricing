using NSubstitute;
using Pricing.Inventory.Application.UseCases.CreateDeviceType;
using Pricing.Inventory.Domain.DeviceTypes;

namespace Pricing.Inventory.Application.UnitTests.UseCases.CreateDeviceType;

public sealed class CreateDeviceTypeUseCaseTests
{
    private const string TypeCode = "SMARTPHONE";
    private const string TypeName = "Smartphone";

    private readonly IDeviceTypeRepository _repository = Substitute.For<IDeviceTypeRepository>();
    private readonly IInventoryUnitOfWork _unitOfWork = Substitute.For<IInventoryUnitOfWork>();
    private readonly CreateDeviceTypeUseCase _sut;

    public CreateDeviceTypeUseCaseTests()
    {
        _sut = new CreateDeviceTypeUseCase(_repository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCodeAlreadyExists_ReturnsFailure()
    {
        // Arrange
        _repository.ExistsByCodeAsync(TypeCode, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _sut.ExecuteAsync(TypeCode, TypeName, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(TypeCode, result.Error);
        await _repository.DidNotReceive().AddAsync(Arg.Any<DeviceType>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCodeIsNew_CreatesTypeAndReturnsResponse()
    {
        // Arrange
        _repository.ExistsByCodeAsync(TypeCode, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _sut.ExecuteAsync(TypeCode, TypeName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(TypeCode, result.Value!.Code);
        Assert.Equal(TypeName, result.Value!.Name);
        await _repository.Received(1).AddAsync(Arg.Any<DeviceType>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("LAPTOP", "Laptop")]
    [InlineData("TABLET", "Tablet")]
    [InlineData("WEARABLE", "Wearable Device")]
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
}
