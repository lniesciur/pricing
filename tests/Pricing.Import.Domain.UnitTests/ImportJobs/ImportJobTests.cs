using Pricing.Import.Domain.ImportJobs;
using Pricing.Shared.Contracts;

namespace Pricing.Import.Domain.UnitTests.ImportJobs;

public class ImportJobTests
{
    private const string FileName = "devices_2024.csv";
    private const FileType DefaultFileType = FileType.Csv;
    private const ImportType DefaultImportType = ImportType.DeviceImport;
    private static readonly byte[] FileContent = [1, 2, 3, 4, 5];

    // -------------------------------------------------------------------------
    // Create
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_WhenCalled_SetsStatusToQueued()
    {
        // Act
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Assert
        Assert.Equal(ImportJobStatus.Queued, job.Status);
    }

    [Fact]
    public void Create_WhenCalled_SetsFileNameProperty()
    {
        // Act
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Assert
        Assert.Equal(FileName, job.FileName);
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public void Create_WhenCalled_SetsFileTypeProperty(FileType fileType)
    {
        // Act
        var job = ImportJob.Create(FileName, fileType, DefaultImportType, FileContent);

        // Assert
        Assert.Equal(fileType, job.FileType);
    }

    [Fact]
    public void Create_WhenCalled_SetsImportTypeProperty()
    {
        // Act
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Assert
        Assert.Equal(DefaultImportType, job.ImportType);
    }

    [Fact]
    public void Create_WhenCalled_AssignsNonEmptyId()
    {
        // Act
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Assert
        Assert.NotNull(job.Id);
        Assert.NotEqual(Guid.Empty, job.Id.Value);
    }

    [Fact]
    public void Create_CalledTwice_ProducesDifferentIds()
    {
        // Act
        var first = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);
        var second = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Assert
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Create_WhenCalled_ErrorsCollectionIsEmpty()
    {
        // Act
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Assert
        Assert.Empty(job.Errors);
    }

    [Fact]
    public void Create_WhenCalled_StatisticsAreAllZero()
    {
        // Act
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Assert
        Assert.Equal(0, job.Added);
        Assert.Equal(0, job.Skipped);
        Assert.Equal(0, job.Updated);
        Assert.Equal(0, job.Deleted);
    }

    [Fact]
    public void Create_WhenCalled_StoresFileContent()
    {
        // Act
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Assert
        Assert.Equal(FileContent, job.FileContent);
    }

    // -------------------------------------------------------------------------
    // MarkAsProcessing
    // -------------------------------------------------------------------------

    [Fact]
    public void MarkAsProcessing_WhenCalled_SetsStatusToProcessing()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Act
        job.MarkAsProcessing();

        // Assert
        Assert.Equal(ImportJobStatus.Processing, job.Status);
    }

    [Fact]
    public void MarkAsProcessing_WhenCalled_DoesNotChangeFileName()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Act
        job.MarkAsProcessing();

        // Assert
        Assert.Equal(FileName, job.FileName);
    }

    [Fact]
    public void MarkAsProcessing_WhenCalled_ErrorsCollectionRemainsEmpty()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Act
        job.MarkAsProcessing();

        // Assert
        Assert.Empty(job.Errors);
    }

    // -------------------------------------------------------------------------
    // MarkAsCompleted — statistics
    // -------------------------------------------------------------------------

    [Fact]
    public void MarkAsCompleted_WhenCalled_SetsStatusToCompleted()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Act
        job.MarkAsCompleted(10, 2, 3, 1, []);

        // Assert
        Assert.Equal(ImportJobStatus.Completed, job.Status);
    }

    [Fact]
    public void MarkAsCompleted_WhenCalled_SetsAddedCount()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Act
        job.MarkAsCompleted(10, 2, 3, 1, []);

        // Assert
        Assert.Equal(10, job.Added);
    }

    [Fact]
    public void MarkAsCompleted_WhenCalled_SetsSkippedCount()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Act
        job.MarkAsCompleted(10, 2, 3, 1, []);

        // Assert
        Assert.Equal(2, job.Skipped);
    }

    [Fact]
    public void MarkAsCompleted_WhenCalled_SetsUpdatedCount()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Act
        job.MarkAsCompleted(10, 2, 3, 1, []);

        // Assert
        Assert.Equal(3, job.Updated);
    }

    [Fact]
    public void MarkAsCompleted_WhenCalled_SetsDeletedCount()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Act
        job.MarkAsCompleted(10, 2, 3, 1, []);

        // Assert
        Assert.Equal(1, job.Deleted);
    }

    // -------------------------------------------------------------------------
    // MarkAsCompleted — empty errors list
    // -------------------------------------------------------------------------

    [Fact]
    public void MarkAsCompleted_WhenEmptyErrorsList_ErrorsCollectionIsEmpty()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Act
        job.MarkAsCompleted(5, 0, 0, 0, []);

        // Assert
        Assert.Empty(job.Errors);
    }

    // -------------------------------------------------------------------------
    // MarkAsCompleted — errors populated
    // -------------------------------------------------------------------------

    [Fact]
    public void MarkAsCompleted_WhenErrorsProvided_AddsAllErrorsToCollection()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);
        List<(int, string, ImportErrorType)> errors =
        [
            (2, "Bad value in column A", ImportErrorType.Parse),
            (5, "Device type not found", ImportErrorType.Domain)
        ];

        // Act
        job.MarkAsCompleted(8, 0, 0, 0, errors);

        // Assert
        Assert.Equal(2, job.Errors.Count);
    }

    [Fact]
    public void MarkAsCompleted_WhenErrorsProvided_FirstErrorHasCorrectRowNumber()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);
        List<(int, string, ImportErrorType)> errors = [(7, "Parse error", ImportErrorType.Parse)];

        // Act
        job.MarkAsCompleted(0, 0, 0, 0, errors);

        // Assert
        Assert.Equal(7, job.Errors[0].RowNumber);
    }

    [Fact]
    public void MarkAsCompleted_WhenErrorsProvided_FirstErrorHasCorrectMessage()
    {
        // Arrange
        const string errorMessage = "Unexpected column count";
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);
        List<(int, string, ImportErrorType)> errors = [(3, errorMessage, ImportErrorType.Parse)];

        // Act
        job.MarkAsCompleted(0, 0, 0, 0, errors);

        // Assert
        Assert.Equal(errorMessage, job.Errors[0].ErrorMessage);
    }

    [Theory]
    [InlineData(ImportErrorType.Parse)]
    [InlineData(ImportErrorType.Domain)]
    public void MarkAsCompleted_WhenErrorsProvided_ErrorTypeIsPreserved(ImportErrorType errorType)
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);
        List<(int, string, ImportErrorType)> errors = [(1, "some error", errorType)];

        // Act
        job.MarkAsCompleted(0, 0, 0, 0, errors);

        // Assert
        Assert.Equal(errorType, job.Errors[0].ErrorType);
    }

    [Fact]
    public void MarkAsCompleted_WhenMultipleErrorsProvided_PreservesOrderAndData()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);
        List<(int, string, ImportErrorType)> errors =
        [
            (2, "Parse error on row 2", ImportErrorType.Parse),
            (9, "Domain error on row 9", ImportErrorType.Domain),
            (15, "Another parse error", ImportErrorType.Parse)
        ];

        // Act
        job.MarkAsCompleted(0, 0, 0, 0, errors);

        // Assert
        Assert.Equal(3, job.Errors.Count);
        Assert.Equal(2, job.Errors[0].RowNumber);
        Assert.Equal(9, job.Errors[1].RowNumber);
        Assert.Equal(15, job.Errors[2].RowNumber);
    }

    // -------------------------------------------------------------------------
    // MarkAsFailed
    // -------------------------------------------------------------------------

    [Fact]
    public void MarkAsFailed_WhenCalled_SetsStatusToFailed()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Act
        job.MarkAsFailed("Could not parse file");

        // Assert
        Assert.Equal(ImportJobStatus.Failed, job.Status);
    }

    [Fact]
    public void MarkAsFailed_WhenCalled_AddsExactlyOneError()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Act
        job.MarkAsFailed("Could not parse file");

        // Assert
        Assert.Single(job.Errors);
    }

    [Fact]
    public void MarkAsFailed_WhenCalled_ErrorHasRowNumberZero()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Act
        job.MarkAsFailed("Could not parse file");

        // Assert
        Assert.Equal(0, job.Errors[0].RowNumber);
    }

    [Fact]
    public void MarkAsFailed_WhenCalled_ErrorTypeIsParse()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Act
        job.MarkAsFailed("Could not parse file");

        // Assert
        Assert.Equal(ImportErrorType.Parse, job.Errors[0].ErrorType);
    }

    [Fact]
    public void MarkAsFailed_WhenCalled_ErrorMessageMatchesProvidedMessage()
    {
        // Arrange
        const string errorMessage = "Unexpected end of file at row 42";
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);

        // Act
        job.MarkAsFailed(errorMessage);

        // Assert
        Assert.Equal(errorMessage, job.Errors[0].ErrorMessage);
    }

    [Fact]
    public void MarkAsFailed_AfterMarkAsProcessing_SetsStatusToFailed()
    {
        // Arrange
        var job = ImportJob.Create(FileName, DefaultFileType, DefaultImportType, FileContent);
        job.MarkAsProcessing();

        // Act
        job.MarkAsFailed("Unexpected error during processing");

        // Assert
        Assert.Equal(ImportJobStatus.Failed, job.Status);
    }
}
