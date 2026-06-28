using System.Text;
using Pricing.Import.Application.FileReading;
using Pricing.Import.Infrastructure.FileReading;

namespace Pricing.Import.Infrastructure.UnitTests.FileReading;

public sealed class CsvFileReaderTests
{
    private static MemoryStream ToCsvStream(string content) =>
        new(Encoding.UTF8.GetBytes(content));

    private readonly CsvFileReader _sut = new();

    public sealed class ProductRow
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    [Fact]
    public async Task ReadAsync_WhenValidCsvWithExpectedHeaders_ReturnsAllRows()
    {
        var csv = "Name,Price\nApple,1.50\nBanana,0.75";
        var options = new FileReaderOptions<ProductRow>
        {
            ExpectedHeaders = ["Name", "Price"]
        };

        var result = await _sut.ReadAsync<ProductRow>(ToCsvStream(csv), options);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("Apple", result.Rows[0].Name);
        Assert.Equal(1.50m, result.Rows[0].Price);
    }

    [Fact]
    public async Task ReadAsync_WhenMissingExpectedHeader_ReturnsErrorAndNoRows()
    {
        var csv = "Name,Cost\nApple,1.50";
        var options = new FileReaderOptions<ProductRow>
        {
            ExpectedHeaders = ["Name", "Price"]
        };

        var result = await _sut.ReadAsync<ProductRow>(ToCsvStream(csv), options);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Rows);
        Assert.Single(result.Errors);
        Assert.Contains("Price", result.Errors[0].Message);
        Assert.Equal(1, result.Errors[0].RowNumber);
    }

    [Fact]
    public async Task ReadAsync_WhenRowFailsValidator_RowAddedToErrorsAndOthersProcessed()
    {
        var csv = "Name,Price\nApple,1.50\nBanana,-1\nCherry,2.00";
        var options = new FileReaderOptions<ProductRow>
        {
            ExpectedHeaders = ["Name", "Price"],
            RowValidator = new NegativePriceValidator()
        };

        var result = await _sut.ReadAsync<ProductRow>(ToCsvStream(csv), options);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Rows.Count);
        Assert.Single(result.Errors);
        Assert.Equal(3, result.Errors[0].RowNumber);
    }

    [Fact]
    public async Task ReadAsync_WhenNoExpectedHeaders_PassesHeaderValidation()
    {
        var csv = "Name,Price\nApple,1.50";
        var options = new FileReaderOptions<ProductRow>();

        var result = await _sut.ReadAsync<ProductRow>(ToCsvStream(csv), options);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Rows);
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
