using Pricing.Import.Application.FileReading;
using Pricing.Import.Domain.ImportJobs;
using Pricing.Inventory.Contracts.Devices;
using Pricing.Inventory.Facade;

namespace Pricing.Import.Application.UseCases.ProcessDeviceImport;

public sealed class ProcessDeviceImportUseCase(
    IImportJobRepository importJobRepository,
    IImportUnitOfWork unitOfWork,
    IFileReader fileReader,
    IInventoryFacade inventoryFacade)
{
    public async Task ExecuteAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await importJobRepository.FindByIdAsync(new ImportJobId(jobId), ct);
        if (job is null)
            return;

        job.MarkAsProcessing();
        await unitOfWork.SaveChangesAsync(ct);

        try
        {
            var (requests, eanToRowNumber, parseErrors) = await ParseFileAsync(job, ct);

            var result = await inventoryFacade.RegisterDevicesAsync(requests, ct);

            var allErrors = new List<(int RowNumber, string Message, ImportErrorType ErrorType)>();
            allErrors.AddRange(parseErrors.Select(e => (e.RowNumber, e.Message, ImportErrorType.Parse)));
            allErrors.AddRange(result.Errors.Select(e => (
                eanToRowNumber.GetValueOrDefault(e.EanCode, 0),
                e.ErrorMessage,
                ImportErrorType.Domain)));

            job.MarkAsCompleted(
                added: result.Added,
                skipped: result.Skipped,
                updated: 0,
                deleted: 0,
                errors: allErrors);
        }
        catch (Exception ex)
        {
            job.MarkAsFailed(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<(
        IReadOnlyList<RegisterDeviceRequest> Requests,
        Dictionary<string, int> EanToRowNumber,
        IReadOnlyList<FileParseError> ParseErrors)>
        ParseFileAsync(ImportJob job, CancellationToken ct)
    {
        var options = new FileReaderOptions<DeviceImportRawRow>
        {
            ExpectedHeaders = ["EanCode", "Name", "TypeCode", "SubtypeCode", "ManufacturerCode"]
        };

        using var stream = new MemoryStream(job.FileContent);
        var requests = new List<RegisterDeviceRequest>();
        var eanToRowNumber = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var parseErrors = new List<FileParseError>();
        var seenEanCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var rowNumber = 0;

        await foreach (var item in fileReader.ReadAsync<DeviceImportRawRow>(stream, job.FileName, options)
                           .WithCancellation(ct))
        {
            rowNumber++;
            if (!item.IsRow)
            {
                parseErrors.Add(item.Error!);
                continue;
            }

            var raw = item.Row!;

            if (string.IsNullOrWhiteSpace(raw.EanCode))
            {
                parseErrors.Add(new FileParseError(rowNumber, "EanCode is required."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(raw.Name))
            {
                parseErrors.Add(new FileParseError(rowNumber, "Name is required."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(raw.TypeCode))
            {
                parseErrors.Add(new FileParseError(rowNumber, "TypeCode is required."));
                continue;
            }

            if (!seenEanCodes.Add(raw.EanCode))
            {
                parseErrors.Add(new FileParseError(rowNumber, $"Duplicate EanCode '{raw.EanCode}' in file."));
                continue;
            }

            var ean = raw.EanCode.Trim();

            var attributes = new List<DeviceAttributeDto>();
            if (!string.IsNullOrWhiteSpace(raw.Color))
                attributes.Add(new DeviceAttributeDto("Color", raw.Color.Trim()));
            if (!string.IsNullOrWhiteSpace(raw.Memory))
                attributes.Add(new DeviceAttributeDto("Memory", raw.Memory.Trim()));

            requests.Add(new RegisterDeviceRequest(
                ean,
                raw.Name.Trim(),
                raw.TypeCode.Trim(),
                string.IsNullOrWhiteSpace(raw.SubtypeCode) ? null : raw.SubtypeCode.Trim(),
                string.IsNullOrWhiteSpace(raw.ManufacturerCode) ? null : raw.ManufacturerCode.Trim(),
                attributes.Count > 0 ? attributes : null));
            eanToRowNumber[ean] = rowNumber;
        }

        return (requests, eanToRowNumber, parseErrors);
    }
}
