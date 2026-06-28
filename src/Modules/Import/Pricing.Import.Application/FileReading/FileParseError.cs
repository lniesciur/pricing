namespace Pricing.Import.Application.FileReading;

public sealed record FileParseError(int RowNumber, string Message);
