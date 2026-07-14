using NSubstitute;
using Pricing.Import.Application;
using Pricing.Import.Application.FileReading;
using Pricing.Import.Application.UseCases.ProcessDeviceImport;
using Pricing.Import.Domain.ImportJobs;
using Pricing.Inventory.Contracts.Devices;
using Pricing.Inventory.Facade;
using Pricing.Shared.Contracts;

namespace Pricing.Import.Application.UnitTests.UseCases.ProcessDeviceImport;

public sealed class ProcessDeviceImportAttributeTests
{
    private readonly IImportJobRepository _importJobRepository = Substitute.For<IImportJobRepository>();
    private readonly IImportUnitOfWork _unitOfWork = Substitute.For<IImportUnitOfWork>();
    private readonly IFileReader _fileReader = Substitute.For<IFileReader>();
    private readonly IInventoryFacade _inventoryFacade = Substitute.For<IInventoryFacade>();
    private readonly ProcessDeviceImportUseCase _sut;

    public ProcessDeviceImportAttributeTests()
    {
        _sut = new ProcessDeviceImportUseCase(
            _importJobRepository,
            _unitOfWork,
            _fileReader,
            _inventoryFacade);

        _inventoryFacade
            .RegisterDevicesAsync(Arg.Any<IReadOnlyList<RegisterDeviceRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new RegisterDevicesResult(0, 0, []));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRowHasColorAndMemory_RegistersDeviceWithBothAttributes()
    {
        var (jobId, _) = CreateJob();
        IReadOnlyList<RegisterDeviceRequest>? capturedRequests = null;
        _inventoryFacade
            .RegisterDevicesAsync(
                Arg.Do<IReadOnlyList<RegisterDeviceRequest>>(r => capturedRequests = r),
                Arg.Any<CancellationToken>())
            .Returns(new RegisterDevicesResult(1, 0, []));

        SetupFileReaderReturning(
            SuccessRow(ean: "EAN001", name: "Phone", typeCode: "SMARTPHONE", color: "Black", memory: "128GB"));

        await _sut.ExecuteAsync(jobId);

        Assert.NotNull(capturedRequests);
        Assert.Single(capturedRequests);
        var request = capturedRequests[0];
        Assert.NotNull(request.Attributes);
        Assert.Equal(2, request.Attributes.Count);
        Assert.Contains(request.Attributes, a => a.Name == "Color" && a.Value == "Black");
        Assert.Contains(request.Attributes, a => a.Name == "Memory" && a.Value == "128GB");
    }

    [Fact]
    public async Task ExecuteAsync_WhenRowHasOnlyColor_RegistersDeviceWithColorAttributeOnly()
    {
        var (jobId, _) = CreateJob();
        IReadOnlyList<RegisterDeviceRequest>? capturedRequests = null;
        _inventoryFacade
            .RegisterDevicesAsync(
                Arg.Do<IReadOnlyList<RegisterDeviceRequest>>(r => capturedRequests = r),
                Arg.Any<CancellationToken>())
            .Returns(new RegisterDevicesResult(1, 0, []));

        SetupFileReaderReturning(
            SuccessRow(ean: "EAN001", name: "Phone", typeCode: "SMARTPHONE", color: "Silver", memory: null));

        await _sut.ExecuteAsync(jobId);

        Assert.NotNull(capturedRequests);
        var request = capturedRequests[0];
        Assert.NotNull(request.Attributes);
        Assert.Single(request.Attributes);
        Assert.Equal("Color", request.Attributes[0].Name);
        Assert.Equal("Silver", request.Attributes[0].Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRowHasOnlyMemory_RegistersDeviceWithMemoryAttributeOnly()
    {
        var (jobId, _) = CreateJob();
        IReadOnlyList<RegisterDeviceRequest>? capturedRequests = null;
        _inventoryFacade
            .RegisterDevicesAsync(
                Arg.Do<IReadOnlyList<RegisterDeviceRequest>>(r => capturedRequests = r),
                Arg.Any<CancellationToken>())
            .Returns(new RegisterDevicesResult(1, 0, []));

        SetupFileReaderReturning(
            SuccessRow(ean: "EAN001", name: "Phone", typeCode: "SMARTPHONE", color: null, memory: "256GB"));

        await _sut.ExecuteAsync(jobId);

        Assert.NotNull(capturedRequests);
        var request = capturedRequests[0];
        Assert.NotNull(request.Attributes);
        Assert.Single(request.Attributes);
        Assert.Equal("Memory", request.Attributes[0].Name);
        Assert.Equal("256GB", request.Attributes[0].Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRowHasNoColorOrMemory_RegistersDeviceWithNullAttributes()
    {
        var (jobId, _) = CreateJob();
        IReadOnlyList<RegisterDeviceRequest>? capturedRequests = null;
        _inventoryFacade
            .RegisterDevicesAsync(
                Arg.Do<IReadOnlyList<RegisterDeviceRequest>>(r => capturedRequests = r),
                Arg.Any<CancellationToken>())
            .Returns(new RegisterDevicesResult(1, 0, []));

        SetupFileReaderReturning(
            SuccessRow(ean: "EAN001", name: "Phone", typeCode: "SMARTPHONE", color: null, memory: null));

        await _sut.ExecuteAsync(jobId);

        Assert.NotNull(capturedRequests);
        Assert.Null(capturedRequests[0].Attributes);
    }

    [Theory]
    [InlineData("   ", null)]
    [InlineData("", null)]
    [InlineData(null, "   ")]
    [InlineData(null, "")]
    [InlineData("  ", "  ")]
    public async Task ExecuteAsync_WhenColorAndMemoryAreBlankOrNull_RegistersDeviceWithNullAttributes(
        string? color, string? memory)
    {
        var (jobId, _) = CreateJob();
        IReadOnlyList<RegisterDeviceRequest>? capturedRequests = null;
        _inventoryFacade
            .RegisterDevicesAsync(
                Arg.Do<IReadOnlyList<RegisterDeviceRequest>>(r => capturedRequests = r),
                Arg.Any<CancellationToken>())
            .Returns(new RegisterDevicesResult(1, 0, []));

        SetupFileReaderReturning(
            SuccessRow(ean: "EAN001", name: "Phone", typeCode: "SMARTPHONE", color: color, memory: memory));

        await _sut.ExecuteAsync(jobId);

        Assert.NotNull(capturedRequests);
        Assert.Null(capturedRequests[0].Attributes);
    }

    [Fact]
    public async Task ExecuteAsync_WhenColorHasSurroundingWhitespace_TrimsColorValue()
    {
        var (jobId, _) = CreateJob();
        IReadOnlyList<RegisterDeviceRequest>? capturedRequests = null;
        _inventoryFacade
            .RegisterDevicesAsync(
                Arg.Do<IReadOnlyList<RegisterDeviceRequest>>(r => capturedRequests = r),
                Arg.Any<CancellationToken>())
            .Returns(new RegisterDevicesResult(1, 0, []));

        SetupFileReaderReturning(
            SuccessRow(ean: "EAN001", name: "Phone", typeCode: "SMARTPHONE", color: "  Black  ", memory: null));

        await _sut.ExecuteAsync(jobId);

        Assert.NotNull(capturedRequests);
        var request = capturedRequests[0];
        Assert.NotNull(request.Attributes);
        Assert.Single(request.Attributes);
        Assert.Equal("Black", request.Attributes[0].Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMemoryHasSurroundingWhitespace_TrimsMemoryValue()
    {
        var (jobId, _) = CreateJob();
        IReadOnlyList<RegisterDeviceRequest>? capturedRequests = null;
        _inventoryFacade
            .RegisterDevicesAsync(
                Arg.Do<IReadOnlyList<RegisterDeviceRequest>>(r => capturedRequests = r),
                Arg.Any<CancellationToken>())
            .Returns(new RegisterDevicesResult(1, 0, []));

        SetupFileReaderReturning(
            SuccessRow(ean: "EAN001", name: "Phone", typeCode: "SMARTPHONE", color: null, memory: " 128GB "));

        await _sut.ExecuteAsync(jobId);

        Assert.NotNull(capturedRequests);
        var request = capturedRequests[0];
        Assert.NotNull(request.Attributes);
        Assert.Single(request.Attributes);
        Assert.Equal("128GB", request.Attributes[0].Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMultipleRowsHaveDifferentAttributeCombinations_MapsAttributesIndependentlyPerRow()
    {
        var (jobId, _) = CreateJob();
        IReadOnlyList<RegisterDeviceRequest>? capturedRequests = null;
        _inventoryFacade
            .RegisterDevicesAsync(
                Arg.Do<IReadOnlyList<RegisterDeviceRequest>>(r => capturedRequests = r),
                Arg.Any<CancellationToken>())
            .Returns(new RegisterDevicesResult(3, 0, []));

        SetupFileReaderReturning(
            SuccessRow(ean: "EAN001", name: "Phone 1", typeCode: "SMARTPHONE", color: "Black", memory: "128GB"),
            SuccessRow(ean: "EAN002", name: "Phone 2", typeCode: "SMARTPHONE", color: "White", memory: null),
            SuccessRow(ean: "EAN003", name: "Phone 3", typeCode: "SMARTPHONE", color: null,    memory: null));

        await _sut.ExecuteAsync(jobId);

        Assert.NotNull(capturedRequests);
        Assert.Equal(3, capturedRequests.Count);

        var req1 = capturedRequests.Single(r => r.EanCode == "EAN001");
        Assert.NotNull(req1.Attributes);
        Assert.Equal(2, req1.Attributes.Count);
        Assert.Contains(req1.Attributes, a => a.Name == "Color" && a.Value == "Black");
        Assert.Contains(req1.Attributes, a => a.Name == "Memory" && a.Value == "128GB");

        var req2 = capturedRequests.Single(r => r.EanCode == "EAN002");
        Assert.NotNull(req2.Attributes);
        Assert.Single(req2.Attributes);
        Assert.Equal("Color", req2.Attributes[0].Name);

        var req3 = capturedRequests.Single(r => r.EanCode == "EAN003");
        Assert.Null(req3.Attributes);
    }

    private (Guid JobId, ImportJob Job) CreateJob(string fileName = "devices.csv")
    {
        var job = ImportJob.Create(fileName, FileType.Csv, ImportType.DeviceImport, Array.Empty<byte>());
        var jobId = Guid.NewGuid();
        _importJobRepository
            .FindByIdAsync(new ImportJobId(jobId), Arg.Any<CancellationToken>())
            .Returns(job);
        return (jobId, job);
    }

    private void SetupFileReaderReturning(params ParsedItem<DeviceImportRawRow>[] items)
    {
        _fileReader
            .ReadAsync<DeviceImportRawRow>(
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                Arg.Any<FileReaderOptions<DeviceImportRawRow>>())
            .Returns(ToAsyncEnumerable(items));
    }

    private static ParsedItem<DeviceImportRawRow> SuccessRow(
        string ean,
        string name,
        string typeCode,
        string? subtypeCode = null,
        string? manufacturerCode = null,
        string? color = null,
        string? memory = null) =>
        ParsedItem<DeviceImportRawRow>.Success(new DeviceImportRawRow
        {
            EanCode = ean,
            Name = name,
            TypeCode = typeCode,
            SubtypeCode = subtypeCode,
            ManufacturerCode = manufacturerCode,
            Color = color,
            Memory = memory
        });

#pragma warning disable CS1998
    private static async IAsyncEnumerable<ParsedItem<DeviceImportRawRow>> ToAsyncEnumerable(
        IEnumerable<ParsedItem<DeviceImportRawRow>> items)
    {
        foreach (var item in items)
            yield return item;
    }
#pragma warning restore CS1998
}
