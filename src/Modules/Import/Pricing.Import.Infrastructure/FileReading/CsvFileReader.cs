using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Pricing.Import.Application.FileReading;

namespace Pricing.Import.Infrastructure.FileReading;

internal sealed class CsvFileReader
{
    public async Task<ParseResult<TRow>> ReadAsync<TRow>(
        Stream stream,
        FileReaderOptions<TRow> options)
        where TRow : class, new()
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
        };

        using var reader = new StreamReader(stream, leaveOpen: true);
        using var csv = new CsvReader(reader, config);

        await csv.ReadAsync();
        csv.ReadHeader();

        var actualHeaders = csv.HeaderRecord ?? [];
        var missingHeaders = options.ExpectedHeaders
            .Where(h => !actualHeaders.Contains(h, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (missingHeaders.Count > 0)
            return new ParseResult<TRow>
            {
                Errors = [new FileParseError(1, $"Missing columns: {string.Join(", ", missingHeaders)}")]
            };

        var rows = new List<TRow>();
        var errors = new List<FileParseError>();
        var rowNumber = 2;

        while (await csv.ReadAsync())
        {
            TRow? row = null;
            try
            {
                row = csv.GetRecord<TRow>();
            }
            catch (Exception ex)
            {
                errors.Add(new FileParseError(rowNumber, ex.Message));
                rowNumber++;
                continue;
            }

            if (row is null)
            {
                rowNumber++;
                continue;
            }

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
}
