using BenchmarkDotNet.Attributes;
using Pricing.Import.Application.FileReading;
using Pricing.Import.Infrastructure.FileReading;

namespace Pricing.Import.PerformanceBenchmarks;

[MemoryDiagnoser]
public class ExcelFileReaderBenchmarks
{
    private readonly ExcelFileReader _reader = new();
    private readonly FileReaderOptions<BenchmarkRow> _options = new()
    {
        ExpectedHeaders = ["Col1", "Col2", "Col3", "Col4", "Col5", "Col6", "Col7", "Col8", "Col9", "Col10"]
    };

    private byte[] _xlsxBytes = null!;

    [Params(1_000, 10_000, 100_000, 500_000)]
    public int RowCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        using var ms = FileFixtureGenerator.GenerateXlsx(RowCount);
        _xlsxBytes = ms.ToArray();
    }

    [Benchmark]
    public async Task<int> ReadAsync()
    {
        using var stream = new MemoryStream(_xlsxBytes);
        var count = 0;
        await foreach (var item in _reader.ReadAsync<BenchmarkRow>(stream, _options))
            if (item.IsRow) count++;
        return count;
    }
}
