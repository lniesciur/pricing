using System.Text;
using MiniExcelLibs;

namespace Pricing.Import.PerformanceBenchmarks;

internal static class FileFixtureGenerator
{
    private static readonly string[] Headers =
        ["Col1", "Col2", "Col3", "Col4", "Col5", "Col6", "Col7", "Col8", "Col9", "Col10"];

    public static MemoryStream GenerateCsv(int rowCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", Headers));
        for (var i = 1; i <= rowCount; i++)
            sb.AppendLine($"val{i}_1,val{i}_2,val{i}_3,val{i}_4,val{i}_5,val{i}_6,val{i}_7,val{i}_8,val{i}_9,val{i}_10");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return new MemoryStream(bytes);
    }

    public static MemoryStream GenerateXlsx(int rowCount)
    {
        var rows = Enumerable.Range(1, rowCount).Select(i => new Dictionary<string, object>
        {
            ["Col1"]  = $"val{i}_1",
            ["Col2"]  = $"val{i}_2",
            ["Col3"]  = $"val{i}_3",
            ["Col4"]  = $"val{i}_4",
            ["Col5"]  = $"val{i}_5",
            ["Col6"]  = $"val{i}_6",
            ["Col7"]  = $"val{i}_7",
            ["Col8"]  = $"val{i}_8",
            ["Col9"]  = $"val{i}_9",
            ["Col10"] = $"val{i}_10",
        });

        var ms = new MemoryStream();
        ms.SaveAs(rows);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }
}
