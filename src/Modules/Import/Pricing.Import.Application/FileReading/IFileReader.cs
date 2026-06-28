namespace Pricing.Import.Application.FileReading;

public interface IFileReader
{
    Task<ParseResult<TRow>> ReadAsync<TRow>(
        Stream stream,
        string fileName,
        FileReaderOptions<TRow> options)
        where TRow : class, new();
}
