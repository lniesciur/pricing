namespace Pricing.Import.Application.UseCases.ProcessDeviceImport;

public class DeviceImportRawRow
{
    public string EanCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TypeCode { get; set; } = string.Empty;
    public string? SubtypeCode { get; set; }
    public string? ManufacturerCode { get; set; }
}
