using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Pricing.Import.Application.FileReading;

namespace Pricing.Import.Infrastructure.FileReading;

internal sealed class CsvFileReader
{
    // ClassMap built once per TRow type via reflection, reused across all CsvReader instances
    private static readonly ConcurrentDictionary<Type, ClassMap> _classMapCache = new();

    public async IAsyncEnumerable<ParsedItem<TRow>> ReadAsync<TRow>(
        Stream stream,
        FileReaderOptions<TRow> options,
        [EnumeratorCancellation] CancellationToken ct = default)
        where TRow : class, new()
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
        };

        using var reader = new StreamReader(stream, leaveOpen: true);
        using var csv = new CsvReader(reader, config);

        if (_classMapCache.TryGetValue(typeof(TRow), out var cachedMap))
            csv.Context.RegisterClassMap(cachedMap);

        await csv.ReadAsync();
        csv.ReadHeader();

        var actualHeaders = csv.HeaderRecord ?? [];
        var missingHeaders = options.ExpectedHeaders
            .Where(h => !actualHeaders.Contains(h, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (missingHeaders.Count > 0)
        {
            yield return ParsedItem<TRow>.Failure(
                new FileParseError(1, $"Missing columns: {string.Join(", ", missingHeaders)}"));
            yield break;
        }

        var rowNumber = 2;
        var mapCached = _classMapCache.ContainsKey(typeof(TRow));

        while (await csv.ReadAsync())
        {
            ct.ThrowIfCancellationRequested();

            TRow? row = null;
            FileParseError? parseError = null;
            try
            {
                row = csv.GetRecord<TRow>();
            }
            catch (Exception ex)
            {
                parseError = new FileParseError(rowNumber, ex.Message);
            }

            if (parseError is not null)
            {
                yield return ParsedItem<TRow>.Failure(parseError);
                rowNumber++;
                continue;
            }

            if (row is null) { rowNumber++; continue; }

            if (!mapCached)
            {
                if (csv.Context.Maps[typeof(TRow)] is { } map)
                    _classMapCache.TryAdd(typeof(TRow), map);
                mapCached = true;
            }

            var validationErrors = options.RowValidator?.Validate(row).ToList() ?? [];
            if (validationErrors.Count > 0)
                foreach (var error in validationErrors)
                    yield return ParsedItem<TRow>.Failure(new FileParseError(rowNumber, error));
            else
                yield return ParsedItem<TRow>.Success(row);

            rowNumber++;
        }
    }
}
