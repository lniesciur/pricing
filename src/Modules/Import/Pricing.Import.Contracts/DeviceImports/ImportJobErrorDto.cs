namespace Pricing.Import.Contracts.DeviceImports;

public record ImportJobErrorDto(int RowNumber, string ErrorMessage, string ErrorType);
