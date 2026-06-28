using MiniExcelLibs;
using Pricing.Import.Application.FileReading;
using Pricing.Import.Infrastructure.FileReading;

namespace Pricing.Import.Infrastructure.UnitTests.FileReading;

public sealed class ExcelFileReaderTests
{
    private static MemoryStream CreateXlsx(IEnumerable<object> rows)
    {
        var ms = new MemoryStream();
        ms.SaveAs(rows);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    private readonly ExcelFileReader _sut = new();

    public sealed class ProductRow
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    [Fact]
    public async Task ReadAsync_WhenValidXlsxWithExpectedHeaders_ReturnsAllRows()
    {
        var ms = CreateXlsx(
        [
            new { Name = "Apple", Price = 1.50m },
            new { Name = "Banana", Price = 0.75m },
        ]);
        var options = new FileReaderOptions<ProductRow>
        {
            ExpectedHeaders = ["Name", "Price"]
        };

        var result = await _sut.ReadAsync<ProductRow>(ms, options);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("Apple", result.Rows[0].Name);
    }

    [Fact]
    public async Task ReadAsync_WhenMissingExpectedHeader_ReturnsErrorAndNoRows()
    {
        var ms = CreateXlsx(
        [
            new { Name = "Apple", Cost = 1.50m },
        ]);
        var options = new FileReaderOptions<ProductRow>
        {
            ExpectedHeaders = ["Name", "Price"]
        };

        var result = await _sut.ReadAsync<ProductRow>(ms, options);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Rows);
        Assert.Single(result.Errors);
        Assert.Contains("Price", result.Errors[0].Message);
    }

    [Fact]
    public async Task ReadAsync_WhenRowFailsValidator_RowAddedToErrorsAndOthersProcessed()
    {
        var ms = CreateXlsx(
        [
            new { Name = "Apple", Price = 1.50m },
            new { Name = "Banana", Price = -1m },
            new { Name = "Cherry", Price = 2.00m },
        ]);
        var options = new FileReaderOptions<ProductRow>
        {
            ExpectedHeaders = ["Name", "Price"],
            RowValidator = new NegativePriceValidator()
        };

        var result = await _sut.ReadAsync<ProductRow>(ms, options);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Rows.Count);
        Assert.Single(result.Errors);
        Assert.Equal(3, result.Errors[0].RowNumber);
    }

    private sealed class NegativePriceValidator : IRowValidator<ProductRow>
    {
        public IEnumerable<string> Validate(ProductRow row)
        {
            if (row.Price < 0)
                yield return "Price must be non-negative.";
        }
    }
}
