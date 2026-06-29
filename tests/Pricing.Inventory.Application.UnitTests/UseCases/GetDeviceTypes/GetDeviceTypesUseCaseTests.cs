using NSubstitute;
using Pricing.Inventory.Application.UseCases.GetDeviceTypes;
using Pricing.Inventory.Domain.DeviceTypes;

namespace Pricing.Inventory.Application.UnitTests.UseCases.GetDeviceTypes;

public sealed class GetDeviceTypesUseCaseTests
{
    private readonly IDeviceTypeRepository _repository = Substitute.For<IDeviceTypeRepository>();
    private readonly GetDeviceTypesUseCase _sut;

    public GetDeviceTypesUseCaseTests()
    {
        _sut = new GetDeviceTypesUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoTypes_ReturnsEmptyList()
    {
        // Arrange
        _repository.FindAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DeviceType>().AsReadOnly());

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Types);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTypesExist_ReturnsMappedDtos()
    {
        // Arrange
        var phone = DeviceType.Create("SMARTPHONE", "Smartphone");
        phone.AddSubtype("IPHONE", "iPhone");
        phone.AddSubtype("ANDROID", "Android Phone");

        var tablet = DeviceType.Create("TABLET", "Tablet");

        _repository.FindAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DeviceType> { phone, tablet }.AsReadOnly());

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Types.Count);

        var phoneDto = result.Value.Types.Single(t => t.Code == "SMARTPHONE");
        Assert.Equal("Smartphone", phoneDto.Name);
        Assert.Equal(2, phoneDto.Subtypes.Count);
        Assert.Contains(phoneDto.Subtypes, s => s.Code == "IPHONE" && s.Name == "iPhone");
        Assert.Contains(phoneDto.Subtypes, s => s.Code == "ANDROID" && s.Name == "Android Phone");

        var tabletDto = result.Value.Types.Single(t => t.Code == "TABLET");
        Assert.Equal("Tablet", tabletDto.Name);
        Assert.Empty(tabletDto.Subtypes);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTypeHasNoSubtypes_MapsToEmptySubtypesList()
    {
        // Arrange
        var deviceType = DeviceType.Create("ACCESSORY", "Accessory");
        _repository.FindAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DeviceType> { deviceType }.AsReadOnly());

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!.Types);
        Assert.Equal("ACCESSORY", dto.Code);
        Assert.Equal("Accessory", dto.Name);
        Assert.Empty(dto.Subtypes);
    }

    [Fact]
    public async Task ExecuteAsync_AlwaysReturnsSuccess()
    {
        // Arrange — even an empty repository is a valid state, not an error
        _repository.FindAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DeviceType>().AsReadOnly());

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
    }
}
