using NSubstitute;
using Pricing.Import.Application.UseCases.GetDeviceImport;
using Pricing.Import.Domain.ImportJobs;
using Pricing.Shared.Contracts;

namespace Pricing.Import.Application.UnitTests.UseCases.GetDeviceImport;

public sealed class GetDeviceImportUseCaseTests
{
    private readonly IImportJobRepository _repository = Substitute.For<IImportJobRepository>();
    private readonly GetDeviceImportUseCase _sut;

    public GetDeviceImportUseCaseTests()
    {
        _sut = new GetDeviceImportUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_WhenJobNotFound_ReturnsFailure()
    {
        var jobId = Guid.NewGuid();
        _repository.FindByIdAsync(Arg.Any<ImportJobId>(), Arg.Any<CancellationToken>()).Returns((ImportJob?)null);

        var result = await _sut.ExecuteAsync(jobId);

        Assert.False(result.IsSuccess);
        Assert.Contains(jobId.ToString(), result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_WhenJobFound_ReturnsResponseWithMatchingData()
    {
        var job = ImportJob.Create("devices.csv", FileType.Csv, ImportType.DeviceImport, []);
        _repository.FindByIdAsync(Arg.Any<ImportJobId>(), Arg.Any<CancellationToken>()).Returns(job);

        var result = await _sut.ExecuteAsync(job.Id.Value);

        Assert.True(result.IsSuccess);
        Assert.Equal(job.Id.Value, result.Value!.JobId);
        Assert.Equal("devices.csv", result.Value!.FileName);
        Assert.Equal(ImportJobStatus.Queued, result.Value!.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenJobIsCompleted_ReturnsStatisticsAndErrors()
    {
        var job = ImportJob.Create("devices.csv", FileType.Csv, ImportType.DeviceImport, []);
        job.MarkAsProcessing();
        job.MarkAsCompleted(
            added: 10, skipped: 2, updated: 0, deleted: 0,
            errors: [(3, "EanCode is required.", ImportErrorType.Parse)]);
        _repository.FindByIdAsync(Arg.Any<ImportJobId>(), Arg.Any<CancellationToken>()).Returns(job);

        var result = await _sut.ExecuteAsync(job.Id.Value);

        Assert.True(result.IsSuccess);
        Assert.Equal(ImportJobStatus.Completed, result.Value!.Status);
        Assert.Equal(10, result.Value!.Added);
        Assert.Equal(2, result.Value!.Skipped);
        Assert.Single(result.Value!.Errors);
        Assert.Equal(3, result.Value!.Errors[0].RowNumber);
    }
}
