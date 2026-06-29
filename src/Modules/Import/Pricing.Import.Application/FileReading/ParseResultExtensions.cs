namespace Pricing.Import.Application.FileReading;

public static class ParseResultExtensions
{
    public static async Task<ParseResult<TRow>> ToParseResultAsync<TRow>(
        this IAsyncEnumerable<ParsedItem<TRow>> items,
        CancellationToken ct = default)
    {
        var rows = new List<TRow>();
        var errors = new List<FileParseError>();
        await foreach (var item in items.WithCancellation(ct))
        {
            if (item.IsRow) rows.Add(item.Row!);
            else errors.Add(item.Error!);
        }
        return new ParseResult<TRow> { Rows = rows, Errors = errors };
    }
}
