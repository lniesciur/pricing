using Pricing.Shared.Contracts;
using Pricing.Shared.Domain;

namespace Pricing.Import.Domain.ImportJobs;

public class ImportJob : AggregateRoot<ImportJobId>
{
    private readonly List<ImportJobError> _errors = [];

    private ImportJob(ImportJobId id, string fileName, FileType fileType, ImportType importType, byte[] fileContent)
        : base(id)
    {
        FileName = fileName;
        FileType = fileType;
        ImportType = importType;
        FileContent = fileContent;
        Status = ImportJobStatus.Queued;
        CreatedAt = DateTime.UtcNow;
    }

    public string FileName { get; private set; }
    public FileType FileType { get; private set; }
    public ImportType ImportType { get; private set; }
    public ImportJobStatus Status { get; private set; }
    public byte[] FileContent { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int Added { get; private set; }
    public int Skipped { get; private set; }
    public int Updated { get; private set; }
    public int Deleted { get; private set; }
    public IReadOnlyList<ImportJobError> Errors => _errors.AsReadOnly();

    public static ImportJob Create(string fileName, FileType fileType, ImportType importType, byte[] fileContent) =>
        new(ImportJobId.New(), fileName, fileType, importType, fileContent);

    public void MarkAsProcessing() =>
        Status = ImportJobStatus.Processing;

    public void MarkAsCompleted(
        int added, int skipped, int updated, int deleted,
        IReadOnlyList<(int RowNumber, string Message, ImportErrorType ErrorType)> errors)
    {
        Status = ImportJobStatus.Completed;
        Added = added;
        Skipped = skipped;
        Updated = updated;
        Deleted = deleted;
        foreach (var (rowNumber, message, errorType) in errors)
            _errors.Add(ImportJobError.Create(rowNumber, message, errorType));
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = ImportJobStatus.Failed;
        _errors.Add(ImportJobError.Create(0, errorMessage, ImportErrorType.Parse));
    }
}
