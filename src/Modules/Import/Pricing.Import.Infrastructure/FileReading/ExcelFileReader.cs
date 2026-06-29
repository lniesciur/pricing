using System.Runtime.CompilerServices;
using MiniExcelLibs;
using Pricing.Import.Application.FileReading;

namespace Pricing.Import.Infrastructure.FileReading;

internal sealed class ExcelFileReader
{
    public async IAsyncEnumerable<ParsedItem<TRow>> ReadAsync<TRow>(
        Stream stream,
        FileReaderOptions<TRow> options,
        [EnumeratorCancellation] CancellationToken ct = default)
        where TRow : class, new()
    {
        var ms = await EnsureSeekableAsync(stream);

        // Pass 1: validate headers (only when caller specified expected headers)
        if (options.ExpectedHeaders.Count > 0)
        {
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
            {
                yield return ParsedItem<TRow>.Failure(
                    new FileParseError(1, $"Missing columns: {string.Join(", ", missingHeaders)}"));
                yield break;
            }

            ms.Seek(0, SeekOrigin.Begin);
        }

        // Pass 2: stream typed rows one at a time
        var rowNumber = 2;
        foreach (var row in await ms.QueryAsync<TRow>())
        {
            ct.ThrowIfCancellationRequested();

            var validationErrors = options.RowValidator?.Validate(row).ToList() ?? [];
            if (validationErrors.Count > 0)
                foreach (var error in validationErrors)
                    yield return ParsedItem<TRow>.Failure(new FileParseError(rowNumber, error));
            else
                yield return ParsedItem<TRow>.Success(row);

            rowNumber++;
        }
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
