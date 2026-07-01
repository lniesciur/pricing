namespace Pricing.Import.Contracts.DeviceImports;

public record ListDeviceImportsResponse(
    IReadOnlyList<DeviceImportSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record DeviceImportSummaryDto(
    Guid JobId,
    string FileName,
    string Status,
    int Added,
    int Skipped,
    int Updated,
    int Deleted,
    DateTime CreatedAt);
