namespace Pricing.Import.Application.FileReading;

public sealed class FileReaderOptions<TRow> where TRow : class
{
    public IReadOnlyList<string> ExpectedHeaders { get; init; } = [];
    public IRowValidator<TRow>? RowValidator { get; init; }
}
