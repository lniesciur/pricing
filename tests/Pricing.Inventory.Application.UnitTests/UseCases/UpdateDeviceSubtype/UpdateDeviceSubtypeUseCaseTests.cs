using NSubstitute;
using Pricing.Inventory.Application.UseCases.UpdateDeviceSubtype;
using Pricing.Inventory.Domain.DeviceTypes;

namespace Pricing.Inventory.Application.UnitTests.UseCases.UpdateDeviceSubtype;

public sealed class UpdateDeviceSubtypeUseCaseTests
{
    private const string TypeCode = "SMARTPHONE";
    private const string TypeName = "Smartphone";
    private const string SubtypeCode = "IPHONE";
    private const string SubtypeName = "iPhone";
    private const string UpdatedSubtypeName = "Apple iPhone";

    private readonly IDeviceTypeRepository _repository = Substitute.For<IDeviceTypeRepository>();
    private readonly IInventoryUnitOfWork _unitOfWork = Substitute.For<IInventoryUnitOfWork>();
    private readonly UpdateDeviceSubtypeUseCase _sut;

    public UpdateDeviceSubtypeUseCaseTests()
    {
        _sut = new UpdateDeviceSubtypeUseCase(_repository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTypeNotFound_ReturnsFailure()
    {
        // Arrange
        _repository.FindByCodeAsync(TypeCode, Arg.Any<CancellationToken>()).Returns((DeviceType?)null);

        // Act
        var result = await _sut.ExecuteAsync(TypeCode, SubtypeCode, UpdatedSubtypeName, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(TypeCode, result.Error);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenSubtypeNotFound_ReturnsFailure()
    {
        // Arrange — type exists but has no subtypes
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        _repository.FindByCodeAsync(TypeCode, Arg.Any<CancellationToken>()).Returns(deviceType);

        // Act
        var result = await _sut.ExecuteAsync(TypeCode, SubtypeCode, UpdatedSubtypeName, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(SubtypeCode, result.Error);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenValid_UpdatesSubtypeNameAndReturnsResponse()
    {
        // Arrange — type exists with the target subtype
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.AddSubtype(SubtypeCode, SubtypeName);
        _repository.FindByCodeAsync(TypeCode, Arg.Any<CancellationToken>()).Returns(deviceType);

        // Act
        var result = await _sut.ExecuteAsync(TypeCode, SubtypeCode, UpdatedSubtypeName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(SubtypeCode, result.Value!.Code);
        Assert.Equal(UpdatedSubtypeName, result.Value!.Name);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenValid_SubtypeCodeIsNotChangedByUpdate()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.AddSubtype(SubtypeCode, SubtypeName);
        _repository.FindByCodeAsync(TypeCode, Arg.Any<CancellationToken>()).Returns(deviceType);

        // Act
        var result = await _sut.ExecuteAsync(TypeCode, SubtypeCode, UpdatedSubtypeName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(SubtypeCode, result.Value!.Code);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTypeHasMultipleSubtypes_OnlyTargetSubtypeIsUpdated()
    {
        // Arrange — type has two subtypes; only one is being renamed
        const string otherSubtypeCode = "ANDROID";
        const string otherSubtypeName = "Android Phone";

        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.AddSubtype(SubtypeCode, SubtypeName);
        deviceType.AddSubtype(otherSubtypeCode, otherSubtypeName);
        _repository.FindByCodeAsync(TypeCode, Arg.Any<CancellationToken>()).Returns(deviceType);

        // Act
        var result = await _sut.ExecuteAsync(TypeCode, SubtypeCode, UpdatedSubtypeName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(SubtypeCode, result.Value!.Code);
        Assert.Equal(UpdatedSubtypeName, result.Value!.Name);
        // The other subtype must remain untouched
        var untouched = deviceType.Subtypes.Single(s => s.Code == otherSubtypeCode);
        Assert.Equal(otherSubtypeName, untouched.Name);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("MISSING_TYPE_X")]
    [InlineData("MISSING_TYPE_Y")]
    public async Task ExecuteAsync_WhenTypeNotFound_ErrorMessageContainsTypeCode(string missingCode)
    {
        // Arrange
        _repository.FindByCodeAsync(missingCode, Arg.Any<CancellationToken>()).Returns((DeviceType?)null);

        // Act
        var result = await _sut.ExecuteAsync(missingCode, SubtypeCode, UpdatedSubtypeName, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(missingCode, result.Error);
    }
}
