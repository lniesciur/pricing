using BenchmarkDotNet.Attributes;
using Pricing.Import.Application.FileReading;
using Pricing.Import.Infrastructure.FileReading;

namespace Pricing.Import.PerformanceBenchmarks;

[MemoryDiagnoser]
public class CsvFileReaderBenchmarks
{
    private readonly CsvFileReader _reader = new();
    private readonly FileReaderOptions<BenchmarkRow> _options = new()
    {
        ExpectedHeaders = ["Col1", "Col2", "Col3", "Col4", "Col5", "Col6", "Col7", "Col8", "Col9", "Col10"]
    };

    private MemoryStream _stream = null!;

    [Params(1_000, 10_000, 100_000, 500_000)]
    public int RowCount { get; set; }

    [GlobalSetup]
    public void Setup() => _stream = FileFixtureGenerator.GenerateCsv(RowCount);

    [GlobalCleanup]
    public void Cleanup() => _stream.Dispose();

    [Benchmark]
    public async Task<int> ReadAsync()
    {
        _stream.Seek(0, SeekOrigin.Begin);
        var count = 0;
        await foreach (var item in _reader.ReadAsync<BenchmarkRow>(_stream, _options))
            if (item.IsRow) count++;
        return count;
    }
}
