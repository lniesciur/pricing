using BenchmarkDotNet.Running;
using Pricing.Import.PerformanceBenchmarks;

BenchmarkRunner.Run(
[
    BenchmarkConverter.TypeToBenchmarks(typeof(CsvFileReaderBenchmarks)),
    BenchmarkConverter.TypeToBenchmarks(typeof(ExcelFileReaderBenchmarks)),
]);
