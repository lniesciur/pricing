using Pricing.Import.Application.FileReading;

namespace Pricing.Import.Infrastructure.FileReading;

internal sealed class FileReaderFacade(CsvFileReader csvReader, ExcelFileReader excelReader) : IFileReader
{
    public Task<ParseResult<TRow>> ReadAsync<TRow>(
        Stream stream,
        string fileName,
        FileReaderOptions<TRow> options)
        where TRow : class, new()
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".csv" => csvReader.ReadAsync<TRow>(stream, options),
            ".xlsx" or ".xls" => excelReader.ReadAsync<TRow>(stream, options),
            _ => throw new NotSupportedException($"File extension '{ext}' is not supported.")
        };
    }
}
