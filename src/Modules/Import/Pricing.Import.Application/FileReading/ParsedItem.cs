namespace Pricing.Import.Application.FileReading;

public readonly struct ParsedItem<TRow>
{
    public TRow? Row { get; }
    public FileParseError? Error { get; }
    public bool IsRow { get; }

    private ParsedItem(TRow? row, FileParseError? error, bool isRow)
    {
        Row = row;
        Error = error;
        IsRow = isRow;
    }

    public static ParsedItem<TRow> Success(TRow row) => new(row, null, true);
    public static ParsedItem<TRow> Failure(FileParseError error) => new(default, error, false);
}
