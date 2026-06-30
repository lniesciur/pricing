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
}
