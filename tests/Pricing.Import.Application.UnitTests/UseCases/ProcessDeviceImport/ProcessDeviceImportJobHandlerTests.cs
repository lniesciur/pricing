using NSubstitute;
using Pricing.Import.Application.FileReading;
using Pricing.Import.Application.UseCases.ProcessDeviceImport;
using Pricing.Import.Domain.ImportJobs;
using Pricing.Inventory.Contracts.Devices;
using Pricing.Inventory.Facade;
using Pricing.Shared.Contracts;

namespace Pricing.Import.Application.UnitTests.UseCases.ProcessDeviceImport;

public sealed class ProcessDeviceImportUseCaseTests
{
    private readonly IImportJobRepository _repository = Substitute.For<IImportJobRepository>();
    private readonly IImportUnitOfWork _unitOfWork = Substitute.For<IImportUnitOfWork>();
    private readonly IFileReader _fileReader = Substitute.For<IFileReader>();
    private readonly IInventoryFacade _inventoryFacade = Substitute.For<IInventoryFacade>();
    private readonly ProcessDeviceImportUseCase _sut;

    public ProcessDeviceImportUseCaseTests()
    {
        _sut = new ProcessDeviceImportUseCase(_repository, _unitOfWork, _fileReader, _inventoryFacade);
    }

    [Fact]
    public async Task ExecuteAsync_WhenJobNotFound_DoesNotSaveOrProcess()
    {
        _repository.FindByIdAsync(Arg.Any<ImportJobId>(), Arg.Any<CancellationToken>()).Returns((ImportJob?)null);

        await _sut.ExecuteAsync(Guid.NewGuid());

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _inventoryFacade.DidNotReceive().RegisterDevicesAsync(Arg.Any<IReadOnlyList<RegisterDeviceRequest>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenFileHasValidRows_MarksJobAsCompleted()
    {
        var job = MakeJob();
        _repository.FindByIdAsync(Arg.Any<ImportJobId>(), Arg.Any<CancellationToken>()).Returns(job);
        _fileReader.ReadAsync<DeviceImportRawRow>(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<FileReaderOptions<DeviceImportRawRow>>())
            .Returns(Items(
                ParsedItem<DeviceImportRawRow>.Success(new DeviceImportRawRow { EanCode = "EAN001", Name = "Phone A", TypeCode = "SMARTPHONE" }),
                ParsedItem<DeviceImportRawRow>.Success(new DeviceImportRawRow { EanCode = "EAN002", Name = "Phone B", TypeCode = "SMARTPHONE" })));
        _inventoryFacade.RegisterDevicesAsync(Arg.Any<IReadOnlyList<RegisterDeviceRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new RegisterDevicesResult(Added: 2, Skipped: 0, Errors: []));

        await _sut.ExecuteAsync(job.Id.Value);

        Assert.Equal(ImportJobStatus.Completed, job.Status);
        Assert.Equal(2, job.Added);
        Assert.Empty(job.Errors);
        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionOccursDuringProcessing_MarksJobAsFailed()
    {
        var job = MakeJob();
        _repository.FindByIdAsync(Arg.Any<ImportJobId>(), Arg.Any<CancellationToken>()).Returns(job);
        _fileReader.ReadAsync<DeviceImportRawRow>(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<FileReaderOptions<DeviceImportRawRow>>())
            .Returns(Items(ParsedItem<DeviceImportRawRow>.Success(new DeviceImportRawRow { EanCode = "EAN001", Name = "Phone", TypeCode = "SMARTPHONE" })));
        _inventoryFacade.RegisterDevicesAsync(Arg.Any<IReadOnlyList<RegisterDeviceRequest>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<RegisterDevicesResult>(new Exception("unexpected failure")));

        await _sut.ExecuteAsync(job.Id.Value);

        Assert.Equal(ImportJobStatus.Failed, job.Status);
        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenFileHasDuplicateEanCode_RejectsSecondRowWithParseError()
    {
        var job = MakeJob();
        _repository.FindByIdAsync(Arg.Any<ImportJobId>(), Arg.Any<CancellationToken>()).Returns(job);
        _fileReader.ReadAsync<DeviceImportRawRow>(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<FileReaderOptions<DeviceImportRawRow>>())
            .Returns(Items(
                ParsedItem<DeviceImportRawRow>.Success(new DeviceImportRawRow { EanCode = "EAN001", Name = "Phone A", TypeCode = "SMARTPHONE" }),
                ParsedItem<DeviceImportRawRow>.Success(new DeviceImportRawRow { EanCode = "EAN001", Name = "Phone B", TypeCode = "SMARTPHONE" })));
        _inventoryFacade.RegisterDevicesAsync(Arg.Any<IReadOnlyList<RegisterDeviceRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new RegisterDevicesResult(Added: 1, Skipped: 0, Errors: []));

        await _sut.ExecuteAsync(job.Id.Value);

        Assert.Equal(ImportJobStatus.Completed, job.Status);
        Assert.Single(job.Errors);
        Assert.Contains("EAN001", job.Errors[0].ErrorMessage);
        Assert.Equal(ImportErrorType.Parse, job.Errors[0].ErrorType);
        await _inventoryFacade.Received(1).RegisterDevicesAsync(
            Arg.Is<IReadOnlyList<RegisterDeviceRequest>>(rows => rows.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenFacadeReturnsDomainErrors_CompletedWithDomainErrorsMappedToRowNumber()
    {
        var job = MakeJob();
        _repository.FindByIdAsync(Arg.Any<ImportJobId>(), Arg.Any<CancellationToken>()).Returns(job);
        _fileReader.ReadAsync<DeviceImportRawRow>(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<FileReaderOptions<DeviceImportRawRow>>())
            .Returns(Items(
                ParsedItem<DeviceImportRawRow>.Success(new DeviceImportRawRow { EanCode = "EAN001", Name = "Phone", TypeCode = "UNKNOWN" })));
        _inventoryFacade.RegisterDevicesAsync(Arg.Any<IReadOnlyList<RegisterDeviceRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new RegisterDevicesResult(
                Added: 0,
                Skipped: 0,
                Errors: [new RegisterDeviceError("EAN001", "TypeCode 'UNKNOWN' not found.")]));

        await _sut.ExecuteAsync(job.Id.Value);

        Assert.Equal(ImportJobStatus.Completed, job.Status);
        Assert.Single(job.Errors);
        Assert.Equal(ImportErrorType.Domain, job.Errors[0].ErrorType);
        Assert.Equal(1, job.Errors[0].RowNumber);
    }

    private static ImportJob MakeJob() =>
        ImportJob.Create("devices.csv", FileType.Csv, ImportType.DeviceImport, []);

    private static async IAsyncEnumerable<ParsedItem<DeviceImportRawRow>> Items(
        params ParsedItem<DeviceImportRawRow>[] items)
    {
        foreach (var item in items)
            yield return item;
        await Task.CompletedTask;
    }
}
