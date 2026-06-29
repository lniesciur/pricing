using NSubstitute;
using Pricing.Inventory.Application.UseCases.AddDeviceSubtype;
using Pricing.Inventory.Domain.DeviceTypes;

namespace Pricing.Inventory.Application.UnitTests.UseCases.AddDeviceSubtype;

public sealed class AddDeviceSubtypeUseCaseTests
{
    private const string TypeCode = "SMARTPHONE";
    private const string TypeName = "Smartphone";
    private const string SubtypeCode = "IPHONE";
    private const string SubtypeName = "iPhone";

    private readonly IDeviceTypeRepository _repository = Substitute.For<IDeviceTypeRepository>();
    private readonly IInventoryUnitOfWork _unitOfWork = Substitute.For<IInventoryUnitOfWork>();
    private readonly AddDeviceSubtypeUseCase _sut;

    public AddDeviceSubtypeUseCaseTests()
    {
        _sut = new AddDeviceSubtypeUseCase(_repository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTypeNotFound_ReturnsFailure()
    {
        // Arrange
        _repository.FindByCodeAsync(TypeCode, Arg.Any<CancellationToken>()).Returns((DeviceType?)null);

        // Act
        var result = await _sut.ExecuteAsync(TypeCode, SubtypeCode, SubtypeName, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(TypeCode, result.Error);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenSubtypeCodeAlreadyExists_ReturnsFailure()
    {
        // Arrange — type exists and already has SubtypeCode registered
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.AddSubtype(SubtypeCode, SubtypeName);
        _repository.FindByCodeAsync(TypeCode, Arg.Any<CancellationToken>()).Returns(deviceType);

        // Act — attempt to add duplicate subtype code
        var result = await _sut.ExecuteAsync(TypeCode, SubtypeCode, "Another iPhone", CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(SubtypeCode, result.Error);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenValid_AddsSubtypeAndReturnsResponse()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        _repository.FindByCodeAsync(TypeCode, Arg.Any<CancellationToken>()).Returns(deviceType);

        // Act
        var result = await _sut.ExecuteAsync(TypeCode, SubtypeCode, SubtypeName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(SubtypeCode, result.Value!.Code);
        Assert.Equal(SubtypeName, result.Value!.Name);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTypeHasExistingSubtypes_AllowsAddingDistinctCode()
    {
        // Arrange — type already has one subtype, adding another with a different code is valid
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.AddSubtype("ANDROID", "Android Phone");
        _repository.FindByCodeAsync(TypeCode, Arg.Any<CancellationToken>()).Returns(deviceType);

        // Act
        var result = await _sut.ExecuteAsync(TypeCode, SubtypeCode, SubtypeName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(SubtypeCode, result.Value!.Code);
        Assert.Equal(SubtypeName, result.Value!.Name);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("MISSING_TYPE_A")]
    [InlineData("MISSING_TYPE_B")]
    public async Task ExecuteAsync_WhenTypeNotFound_ErrorMessageContainsTypeCode(string missingCode)
    {
        // Arrange
        _repository.FindByCodeAsync(missingCode, Arg.Any<CancellationToken>()).Returns((DeviceType?)null);

        // Act
        var result = await _sut.ExecuteAsync(missingCode, SubtypeCode, SubtypeName, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(missingCode, result.Error);
    }
}
