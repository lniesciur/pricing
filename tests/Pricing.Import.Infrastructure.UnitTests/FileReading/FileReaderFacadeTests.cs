using System.Text;
using MiniExcelLibs;
using Pricing.Import.Application.FileReading;
using Pricing.Import.Infrastructure.FileReading;

namespace Pricing.Import.Infrastructure.UnitTests.FileReading;

public sealed class FileReaderFacadeTests
{
    public sealed class SampleRow
    {
        public string Name { get; set; } = string.Empty;
    }

    private static FileReaderFacade BuildFacade() =>
        new(new CsvFileReader(), new ExcelFileReader());

    private static MemoryStream ToCsvStream(string content) =>
        new(Encoding.UTF8.GetBytes(content));

    private static MemoryStream CreateXlsx(IEnumerable<object> rows)
    {
        var ms = new MemoryStream();
        ms.SaveAs(rows);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    [Fact]
    public async Task ReadAsync_WhenFileNameIsCsv_DelegatesToCsvFileReader()
    {
        var stream = ToCsvStream("Name\nApple");
        var options = new FileReaderOptions<SampleRow> { ExpectedHeaders = ["Name"] };

        var result = await BuildFacade().ReadAsync<SampleRow>(stream, "products.csv", options);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Rows);
    }

    [Fact]
    public async Task ReadAsync_WhenFileNameIsXlsx_DelegatesToExcelFileReader()
    {
        var stream = CreateXlsx([new { Name = "Apple" }]);
        var options = new FileReaderOptions<SampleRow> { ExpectedHeaders = ["Name"] };

        var result = await BuildFacade().ReadAsync<SampleRow>(stream, "products.xlsx", options);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Rows);
    }

    [Fact]
    public async Task ReadAsync_WhenFileNameIsXls_DelegatesToExcelFileReader()
    {
        // xls extension is routed to ExcelFileReader (MiniExcel handles format detection)
        var stream = CreateXlsx([new { Name = "Apple" }]);
        var options = new FileReaderOptions<SampleRow> { ExpectedHeaders = ["Name"] };

        var result = await BuildFacade().ReadAsync<SampleRow>(stream, "products.xls", options);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ReadAsync_WhenUnknownExtension_ThrowsNotSupportedException()
    {
        var options = new FileReaderOptions<SampleRow>();

        await Assert.ThrowsAsync<NotSupportedException>(() =>
            BuildFacade().ReadAsync<SampleRow>(new MemoryStream(), "data.ods", options));
    }
}
