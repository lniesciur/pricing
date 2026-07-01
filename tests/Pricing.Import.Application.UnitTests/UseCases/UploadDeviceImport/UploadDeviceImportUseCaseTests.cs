using NSubstitute;
using Pricing.Import.Application.UseCases.UploadDeviceImport;
using Pricing.Import.Domain.ImportJobs;
using Pricing.Shared.Contracts;

namespace Pricing.Import.Application.UnitTests.UseCases.UploadDeviceImport;

public sealed class UploadDeviceImportUseCaseTests
{
    private readonly IImportJobRepository _repository = Substitute.For<IImportJobRepository>();
    private readonly IImportUnitOfWork _unitOfWork = Substitute.For<IImportUnitOfWork>();
    private readonly IImportJobScheduler _scheduler = Substitute.For<IImportJobScheduler>();
    private readonly UploadDeviceImportUseCase _sut;

    public UploadDeviceImportUseCaseTests()
    {
        _sut = new UploadDeviceImportUseCase(_repository, _unitOfWork, _scheduler);
    }

    [Theory]
    [InlineData("import.pdf")]
    [InlineData("import.txt")]
    [InlineData("import")]
    public async Task ExecuteAsync_WhenFileExtensionIsUnsupported_ReturnsFailure(string fileName)
    {
        var stream = new MemoryStream([1, 2, 3]);

        var result = await _sut.ExecuteAsync(fileName, stream);

        Assert.False(result.IsSuccess);
        await _repository.DidNotReceive().AddAsync(Arg.Any<ImportJob>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _scheduler.DidNotReceive().EnqueueDeviceImportProcessing(Arg.Any<Guid>());
    }

    [Theory]
    [InlineData("devices.csv")]
    [InlineData("devices.CSV")]
    [InlineData("devices.xlsx")]
    [InlineData("devices.XLSX")]
    public async Task ExecuteAsync_WhenSupportedFileIsProvided_SavesJobAndSchedulesProcessing(string fileName)
    {
        var stream = new MemoryStream([1, 2, 3]);

        var result = await _sut.ExecuteAsync(fileName, stream);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value!.JobId);
        await _repository.Received(1).AddAsync(Arg.Any<ImportJob>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _scheduler.Received(1).EnqueueDeviceImportProcessing(result.Value!.JobId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCsvFileIsProvided_CreatesJobWithCorrectFileType()
    {
        var stream = new MemoryStream([1, 2, 3]);
        ImportJob? capturedJob = null;
        await _repository.AddAsync(
            Arg.Do<ImportJob>(j => capturedJob = j),
            Arg.Any<CancellationToken>());

        await _sut.ExecuteAsync("devices.csv", stream);

        Assert.Equal(FileType.Csv, capturedJob!.FileType);
        Assert.Equal("devices.csv", capturedJob.FileName);
        Assert.Equal(ImportJobStatus.Queued, capturedJob.Status);
    }
}
