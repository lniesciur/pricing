namespace Pricing.Import.Application.FileReading;

public sealed class ParseResult<TRow>
{
    public IReadOnlyList<TRow> Rows { get; init; } = [];
    public IReadOnlyList<FileParseError> Errors { get; init; } = [];
    public bool IsSuccess => Errors.Count == 0;
}
