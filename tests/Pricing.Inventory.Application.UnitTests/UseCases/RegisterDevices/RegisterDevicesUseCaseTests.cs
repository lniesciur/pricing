using NSubstitute;
using Pricing.Inventory.Application.UseCases.RegisterDevices;
using Pricing.Inventory.Contracts.Devices;
using Pricing.Inventory.Domain.DeviceTypes;
using Pricing.Inventory.Domain.Manufacturers;

namespace Pricing.Inventory.Application.UnitTests.UseCases.RegisterDevices;

public sealed class RegisterDevicesUseCaseTests
{
    private readonly IDeviceTypeRepository _deviceTypeRepository = Substitute.For<IDeviceTypeRepository>();
    private readonly IManufacturerRepository _manufacturerRepository = Substitute.For<IManufacturerRepository>();
    private readonly RegisterDevicesUseCase _sut;

    public RegisterDevicesUseCaseTests()
    {
        _sut = new RegisterDevicesUseCase(_deviceTypeRepository, _manufacturerRepository);
    }

    // -------------------------------------------------------------------------
    // Pre-existing validation tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenRowHasUnknownTypeCode_ReturnsErrorAndExcludesRow()
    {
        SetupRepositories(types: [], manufacturers: []);
        var requests = new[] { Request("EAN001", typeCode: "UNKNOWN") };

        var result = await _sut.ExecuteAsync(requests);

        Assert.Empty(result.ValidDevices);
        Assert.Single(result.Errors);
        Assert.Equal("EAN001", result.Errors[0].EanCode);
        Assert.Contains("UNKNOWN", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRowHasUnknownSubtypeCode_ReturnsErrorAndExcludesRow()
    {
        var type = DeviceType.Create("SMARTPHONE", "Smartphone");
        type.AddSubtype("ANDROID", "Android");
        SetupRepositories(types: [type], manufacturers: []);
        var requests = new[] { Request("EAN001", typeCode: "SMARTPHONE", subtypeCode: "IOS") };

        var result = await _sut.ExecuteAsync(requests);

        Assert.Empty(result.ValidDevices);
        Assert.Single(result.Errors);
        Assert.Contains("IOS", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRowHasUnknownManufacturerCode_ReturnsErrorAndExcludesRow()
    {
        var type = DeviceType.Create("SMARTPHONE", "Smartphone");
        SetupRepositories(types: [type], manufacturers: []);
        var requests = new[] { Request("EAN001", typeCode: "SMARTPHONE", manufacturerCode: "UNKNOWN_MFR") };

        var result = await _sut.ExecuteAsync(requests);

        Assert.Empty(result.ValidDevices);
        Assert.Single(result.Errors);
        Assert.Contains("UNKNOWN_MFR", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAllRowsAreValid_ReturnsAllDevicesWithNoErrors()
    {
        var type = DeviceType.Create("SMARTPHONE", "Smartphone");
        type.AddSubtype("ANDROID", "Android");
        var manufacturer = Manufacturer.Create("SAMSUNG", "Samsung");
        SetupRepositories(types: [type], manufacturers: [manufacturer]);
        var requests = new[]
        {
            Request("EAN001", typeCode: "SMARTPHONE", subtypeCode: "ANDROID", manufacturerCode: "SAMSUNG"),
            Request("EAN002", typeCode: "SMARTPHONE"),
            Request("EAN003", typeCode: "SMARTPHONE", manufacturerCode: "SAMSUNG"),
        };

        var result = await _sut.ExecuteAsync(requests);

        Assert.Equal(3, result.ValidDevices.Count);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMixedValidAndInvalidRows_ReturnsOnlyValidDevices()
    {
        var type = DeviceType.Create("SMARTPHONE", "Smartphone");
        SetupRepositories(types: [type], manufacturers: []);
        var requests = new[]
        {
            Request("EAN001", typeCode: "SMARTPHONE"),
            Request("EAN002", typeCode: "TABLET"),
            Request("EAN003", typeCode: "SMARTPHONE"),
        };

        var result = await _sut.ExecuteAsync(requests);

        Assert.Equal(2, result.ValidDevices.Count);
        Assert.Single(result.Errors);
        Assert.Equal("EAN002", result.Errors[0].EanCode);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRowSubtypeCodeIsNull_AcceptsRowRegardlessOfSubtypes()
    {
        var type = DeviceType.Create("SMARTPHONE", "Smartphone");
        type.AddSubtype("ANDROID", "Android");
        SetupRepositories(types: [type], manufacturers: []);
        var requests = new[] { Request("EAN001", typeCode: "SMARTPHONE", subtypeCode: null) };

        var result = await _sut.ExecuteAsync(requests);

        Assert.Single(result.ValidDevices);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidRequest_CreatesDeviceWithCorrectData()
    {
        var type = DeviceType.Create("SMARTPHONE", "Smartphone");
        var manufacturer = Manufacturer.Create("SAMSUNG", "Samsung");
        SetupRepositories(types: [type], manufacturers: [manufacturer]);
        var requests = new[] { Request("EAN001", typeCode: "SMARTPHONE", manufacturerCode: "SAMSUNG") };

        var result = await _sut.ExecuteAsync(requests);

        Assert.Single(result.ValidDevices);
        var device = result.ValidDevices[0];
        Assert.Equal("EAN001", device.EanCode);
        Assert.Equal("SMARTPHONE", device.TypeCode);
        Assert.Equal("SAMSUNG", device.ManufacturerCode);
    }

    // -------------------------------------------------------------------------
    // SPEC-007: Device Attributes
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenRequestHasAttributes_CreatesDeviceWithMappedAttributes()
    {
        // Arrange
        const string EanCode = "EAN001";
        var type = DeviceType.Create("SMARTPHONE", "Smartphone");
        SetupRepositories(types: [type], manufacturers: []);
        var attributeDtos = new List<DeviceAttributeDto>
        {
            new("Color", "Black"),
            new("Memory", "128GB")
        };
        var requests = new[] { RequestWithAttributes(EanCode, attributes: attributeDtos) };

        // Act
        var result = await _sut.ExecuteAsync(requests);

        // Assert
        Assert.Single(result.ValidDevices);
        Assert.Empty(result.Errors);
        var device = result.ValidDevices[0];
        Assert.Equal(2, device.Attributes.Count);
        Assert.Contains(device.Attributes, a => a.Name == "Color" && a.Value == "Black");
        Assert.Contains(device.Attributes, a => a.Name == "Memory" && a.Value == "128GB");
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestHasSingleAttribute_CreatesDeviceWithThatAttribute()
    {
        // Arrange
        var type = DeviceType.Create("SMARTPHONE", "Smartphone");
        SetupRepositories(types: [type], manufacturers: []);
        var attributeDtos = new List<DeviceAttributeDto> { new("Color", "Silver") };
        var requests = new[] { RequestWithAttributes("EAN001", attributes: attributeDtos) };

        // Act
        var result = await _sut.ExecuteAsync(requests);

        // Assert
        Assert.Single(result.ValidDevices);
        var device = result.ValidDevices[0];
        Assert.Single(device.Attributes);
        Assert.Equal("Color", device.Attributes[0].Name);
        Assert.Equal("Silver", device.Attributes[0].Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestHasNullAttributes_CreatesDeviceWithEmptyAttributes()
    {
        // Arrange
        var type = DeviceType.Create("SMARTPHONE", "Smartphone");
        SetupRepositories(types: [type], manufacturers: []);
        var requests = new[] { Request("EAN001") };

        // Act
        var result = await _sut.ExecuteAsync(requests);

        // Assert
        Assert.Single(result.ValidDevices);
        Assert.Empty(result.Errors);
        Assert.Empty(result.ValidDevices[0].Attributes);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestHasDuplicateAttributeNames_ThrowsInvalidOperationException()
    {
        // Arrange
        var type = DeviceType.Create("SMARTPHONE", "Smartphone");
        SetupRepositories(types: [type], manufacturers: []);
        var attributeDtos = new List<DeviceAttributeDto>
        {
            new("Color", "Black"),
            new("color", "White")  // case-insensitive duplicate
        };
        var requests = new[] { RequestWithAttributes("EAN001", attributes: attributeDtos) };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ExecuteAsync(requests));
    }

    [Theory]
    [InlineData("COLOR", "color")]
    [InlineData("Memory", "MEMORY")]
    [InlineData("Brand", "brand")]
    public async Task ExecuteAsync_WhenDuplicateAttributeNamesDifferInCase_ThrowsInvalidOperationException(
        string firstName, string secondName)
    {
        // Arrange
        var type = DeviceType.Create("SMARTPHONE", "Smartphone");
        SetupRepositories(types: [type], manufacturers: []);
        var attributeDtos = new List<DeviceAttributeDto>
        {
            new(firstName, "Value1"),
            new(secondName, "Value2")
        };
        var requests = new[] { RequestWithAttributes("EAN001", attributes: attributeDtos) };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ExecuteAsync(requests));
    }

    [Fact]
    public async Task ExecuteAsync_WhenMultipleRequestsHaveAttributes_MapsAttributesPerDevice()
    {
        // Arrange
        var type = DeviceType.Create("SMARTPHONE", "Smartphone");
        SetupRepositories(types: [type], manufacturers: []);
        var requests = new[]
        {
            RequestWithAttributes("EAN001", attributes: [new DeviceAttributeDto("Color", "Black")]),
            RequestWithAttributes("EAN002", attributes: [new DeviceAttributeDto("Color", "White"), new DeviceAttributeDto("Memory", "256GB")]),
            Request("EAN003")  // no attributes
        };

        // Act
        var result = await _sut.ExecuteAsync(requests);

        // Assert
        Assert.Equal(3, result.ValidDevices.Count);
        Assert.Empty(result.Errors);

        var device1 = result.ValidDevices.Single(d => d.EanCode == "EAN001");
        Assert.Single(device1.Attributes);
        Assert.Equal("Black", device1.Attributes[0].Value);

        var device2 = result.ValidDevices.Single(d => d.EanCode == "EAN002");
        Assert.Equal(2, device2.Attributes.Count);

        var device3 = result.ValidDevices.Single(d => d.EanCode == "EAN003");
        Assert.Empty(device3.Attributes);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void SetupRepositories(IEnumerable<DeviceType> types, IEnumerable<Manufacturer> manufacturers)
    {
        _deviceTypeRepository.FindAllAsync(Arg.Any<CancellationToken>())
            .Returns(types.ToList());
        _manufacturerRepository.FindAllAsync(Arg.Any<CancellationToken>())
            .Returns(manufacturers.ToList());
    }

    private static RegisterDeviceRequest Request(
        string eanCode,
        string typeCode = "SMARTPHONE",
        string? subtypeCode = null,
        string? manufacturerCode = null) =>
        new(eanCode, $"Device {eanCode}", typeCode, subtypeCode, manufacturerCode);

    private static RegisterDeviceRequest RequestWithAttributes(
        string eanCode,
        string typeCode = "SMARTPHONE",
        string? subtypeCode = null,
        string? manufacturerCode = null,
        IReadOnlyList<DeviceAttributeDto>? attributes = null) =>
        new(eanCode, $"Device {eanCode}", typeCode, subtypeCode, manufacturerCode, attributes);
}
