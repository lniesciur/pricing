using Pricing.Shared.Domain;

namespace Pricing.Import.Domain.ImportJobs;

public class ImportJobError : Entity<ImportJobErrorId>
{
    private ImportJobError(ImportJobErrorId id, int rowNumber, string errorMessage, ImportErrorType errorType)
        : base(id)
    {
        RowNumber = rowNumber;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
    }

    public int RowNumber { get; private set; }
    public string ErrorMessage { get; private set; }
    public ImportErrorType ErrorType { get; private set; }

    internal static ImportJobError Create(int rowNumber, string errorMessage, ImportErrorType errorType) =>
        new(ImportJobErrorId.New(), rowNumber, errorMessage, errorType);
}
