using Pricing.Shared.Contracts;

namespace Pricing.Import.Contracts.DeviceImports;

public record GetDeviceImportResponse(
    Guid JobId,
    string FileName,
    ImportJobStatus Status,
    ImportType ImportType,
    int Added,
    int Skipped,
    int Updated,
    int Deleted,
    IReadOnlyList<ImportJobErrorDto> Errors);
