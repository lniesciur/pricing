using MiniExcelLibs;
using Pricing.Import.Application.FileReading;

namespace Pricing.Import.Infrastructure.FileReading;

internal sealed class ExcelFileReader
{
    public async Task<ParseResult<TRow>> ReadAsync<TRow>(
        Stream stream,
        FileReaderOptions<TRow> options)
        where TRow : class, new()
    {
        var ms = await EnsureSeekableAsync(stream);

        // Pass 1: extract column names via dynamic (useHeaderRow: true → keys are column names)
        List<string> actualHeaders = [];
        var headerRows = await ms.QueryAsync(useHeaderRow: true);
        foreach (dynamic row in headerRows)
        {
            IDictionary<string, object?> dict = row;
            actualHeaders = [.. dict.Keys];
            break;
        }

        var missingHeaders = options.ExpectedHeaders
            .Where(h => !actualHeaders.Contains(h, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (missingHeaders.Count > 0)
            return new ParseResult<TRow>
            {
                Errors = [new FileParseError(1, $"Missing columns: {string.Join(", ", missingHeaders)}")]
            };

        // Pass 2: typed rows (QueryAsync<T> always uses header row for property mapping)
        ms.Seek(0, SeekOrigin.Begin);

        var rows = new List<TRow>();
        var errors = new List<FileParseError>();
        var rowNumber = 2;

        foreach (var row in await ms.QueryAsync<TRow>())
        {
            var validationErrors = options.RowValidator?.Validate(row).ToList() ?? [];
            if (validationErrors.Count > 0)
                foreach (var error in validationErrors)
                    errors.Add(new FileParseError(rowNumber, error));
            else
                rows.Add(row);

            rowNumber++;
        }

        return new ParseResult<TRow> { Rows = rows, Errors = errors };
    }

    private static async Task<MemoryStream> EnsureSeekableAsync(Stream stream)
    {
        if (stream is MemoryStream ms && ms.CanSeek)
        {
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);
        buffer.Seek(0, SeekOrigin.Begin);
        return buffer;
    }
}
