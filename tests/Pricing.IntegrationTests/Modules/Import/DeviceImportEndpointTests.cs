using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MiniExcelLibs;
using Pricing.Import.Contracts.DeviceImports;
using Pricing.Import.Infrastructure.Persistence;
using Pricing.IntegrationTests.Infrastructure;
using Pricing.Shared.Contracts;

namespace Pricing.IntegrationTests.Modules.Import;

public class DeviceImportEndpointTests : IClassFixture<ApiFactory>, IAsyncDisposable
{
    private readonly HttpClient _client;
    private readonly ApiFactory _factory;
    private readonly List<Guid> _createdJobIds = [];

    public DeviceImportEndpointTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ── Upload endpoint (HTTP layer only) ────────────────────────────────────

    [Fact]
    public async Task UploadDeviceImport_WithValidCsvFile_Returns202WithJobId()
    {
        using var content = BuildCsvMultipart("devices.csv", "EanCode,Name,TypeCode,SubtypeCode,ManufacturerCode\nEAN001,Device A,LAPTOP,,");

        var response = await _client.PostAsync("/api/import/device-imports", content);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UploadDeviceImportResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.JobId);
        _createdJobIds.Add(body.JobId);
    }

    [Fact]
    public async Task UploadDeviceImport_WithXlsxContentType_Returns202WithJobId()
    {
        using var content = BuildMultipart("devices.xlsx", "placeholder", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        var response = await _client.PostAsync("/api/import/device-imports", content);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UploadDeviceImportResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.JobId);
        _createdJobIds.Add(body.JobId);
    }

    [Fact]
    public async Task UploadDeviceImport_WithUnsupportedExtension_Returns422()
    {
        using var content = BuildMultipart("devices.txt", "some content");

        var response = await _client.PostAsync("/api/import/device-imports", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Processing pipeline (end-to-end: upload → Hangfire → terminal status) ─

    [Fact]
    public async Task ProcessDeviceImport_WithValidCsv_CompletesWithAddedDevices()
    {
        var csv = "EanCode,Name,TypeCode,SubtypeCode,ManufacturerCode\nEAN-E2E-001,Device A,LAPTOP,,\nEAN-E2E-002,Device B,PHONE,,";
        using var content = BuildCsvMultipart("e2e-valid.csv", csv);

        var uploadResponse = await _client.PostAsync("/api/import/device-imports", content);
        var body = await uploadResponse.Content.ReadFromJsonAsync<UploadDeviceImportResponse>();
        _createdJobIds.Add(body!.JobId);

        var job = await GetJobAsync(body.JobId);

        Assert.Equal(ImportJobStatus.Completed, job.Status);
        Assert.Equal(2, job.Added);
        Assert.Empty(job.Errors);
    }

    [Fact]
    public async Task ProcessDeviceImport_WithDuplicateEanInFile_CompletesWithRowError()
    {
        var csv = "EanCode,Name,TypeCode,SubtypeCode,ManufacturerCode\nEAN-DUP-001,Device A,LAPTOP,,\nEAN-DUP-001,Device B,PHONE,,";
        using var content = BuildCsvMultipart("e2e-dup.csv", csv);

        var uploadResponse = await _client.PostAsync("/api/import/device-imports", content);
        var body = await uploadResponse.Content.ReadFromJsonAsync<UploadDeviceImportResponse>();
        _createdJobIds.Add(body!.JobId);

        var job = await GetJobAsync(body.JobId);

        Assert.Equal(ImportJobStatus.Completed, job.Status);
        Assert.Equal(1, job.Added);
        Assert.Single(job.Errors);
        Assert.Contains("Duplicate EanCode", job.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ProcessDeviceImport_WithValidXlsx_CompletesWithAddedDevice()
    {
        using var content = BuildValidXlsxMultipart("e2e-valid.xlsx",
            new { EanCode = "EAN-XLSX-001", Name = "Xlsx Device", TypeCode = "LAPTOP", SubtypeCode = "", ManufacturerCode = "" });

        var uploadResponse = await _client.PostAsync("/api/import/device-imports", content);
        var body = await uploadResponse.Content.ReadFromJsonAsync<UploadDeviceImportResponse>();
        _createdJobIds.Add(body!.JobId);

        var job = await GetJobAsync(body.JobId);

        Assert.Equal(ImportJobStatus.Completed, job.Status);
        Assert.Equal(1, job.Added);
        Assert.Empty(job.Errors);
    }

    [Fact]
    public async Task ProcessDeviceImport_WithCorruptXlsxContent_MarksJobAsFailed()
    {
        using var content = BuildMultipart("e2e-corrupt.xlsx", "not-an-xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        var uploadResponse = await _client.PostAsync("/api/import/device-imports", content);
        var body = await uploadResponse.Content.ReadFromJsonAsync<UploadDeviceImportResponse>();
        _createdJobIds.Add(body!.JobId);

        var job = await GetJobAsync(body.JobId);

        Assert.Equal(ImportJobStatus.Failed, job.Status);
    }

    // ── Query endpoints ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetDeviceImport_WhenJobExists_Returns200WithDetails()
    {
        var jobId = await UploadAndGetJobId("get-test.csv");

        var response = await _client.GetAsync($"/api/import/device-imports/{jobId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetDeviceImportResponse>();
        Assert.NotNull(body);
        Assert.Equal(jobId, body.JobId);
        Assert.Equal("get-test.csv", body.FileName);
        Assert.Equal(ImportType.DeviceImport, body.ImportType);
    }

    [Fact]
    public async Task GetDeviceImport_WhenJobNotFound_Returns404()
    {
        var response = await _client.GetAsync($"/api/import/device-imports/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListDeviceImports_AfterUpload_ReturnsJobInList()
    {
        var jobId = await UploadAndGetJobId("list-test.csv");

        var response = await _client.GetAsync("/api/import/device-imports");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ListDeviceImportsResponse>();
        Assert.NotNull(body);
        Assert.Contains(body.Items, item => item.JobId == jobId);
    }

    [Fact]
    public async Task ListDeviceImports_WithStatusFilter_ReturnsOnlyMatchingJobs()
    {
        // With SynchronousImportJobScheduler the job is Completed immediately after upload.
        var jobId = await UploadAndGetJobId("filter-test.csv");

        var response = await _client.GetAsync($"/api/import/device-imports?status={ImportJobStatus.Completed}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ListDeviceImportsResponse>();
        Assert.NotNull(body);
        Assert.All(body.Items, item => Assert.Equal(nameof(ImportJobStatus.Completed), item.Status));
        Assert.Contains(body.Items, item => item.JobId == jobId);
    }

    [Fact]
    public async Task ListDeviceImports_WithPagination_RespjectsPageSize()
    {
        var id1 = await UploadAndGetJobId("page-test-1.csv");
        var id2 = await UploadAndGetJobId("page-test-2.csv");
        var id3 = await UploadAndGetJobId("page-test-3.csv");

        var response = await _client.GetAsync("/api/import/device-imports?page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ListDeviceImportsResponse>();
        Assert.NotNull(body);
        Assert.Equal(2, body.Items.Count);
        Assert.Equal(1, body.Page);
        Assert.Equal(2, body.PageSize);
        _ = (id1, id2, id3);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<Guid> UploadAndGetJobId(string fileName)
    {
        using var content = BuildCsvMultipart(fileName, "EanCode,Name,TypeCode,SubtypeCode,ManufacturerCode\nEAN001,Device,LAPTOP,,");
        var response = await _client.PostAsync("/api/import/device-imports", content);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<UploadDeviceImportResponse>();
        _createdJobIds.Add(body!.JobId);
        return body.JobId;
    }

    private async Task<GetDeviceImportResponse> GetJobAsync(Guid jobId)
    {
        var response = await _client.GetAsync($"/api/import/device-imports/{jobId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GetDeviceImportResponse>())!;
    }

    private static MultipartFormDataContent BuildCsvMultipart(string fileName, string csvContent)
        => BuildMultipart(fileName, csvContent, "text/csv");

    private static MultipartFormDataContent BuildMultipart(string fileName, string fileContent, string mediaType = "text/csv")
    {
        var content = new MultipartFormDataContent();
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(fileContent);
        var fileStreamContent = new ByteArrayContent(fileBytes);
        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        content.Add(fileStreamContent, "file", fileName);
        return content;
    }

    private static MultipartFormDataContent BuildValidXlsxMultipart(string fileName, object row)
    {
        var stream = new MemoryStream();
        stream.SaveAs(new[] { row }, excelType: ExcelType.XLSX);
        stream.Seek(0, SeekOrigin.Begin);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(stream.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "file", fileName);
        return content;
    }

    public async ValueTask DisposeAsync()
    {
        if (_createdJobIds.Count > 0)
        {
            await using var cleanupScope = _factory.Services.CreateAsyncScope();
            var cleanupDb = cleanupScope.ServiceProvider.GetRequiredService<ImportDbContext>();
            var toDelete = cleanupDb.ImportJobs.Where(j => _createdJobIds.Contains(j.Id.Value)).ToList();
            if (toDelete.Count > 0)
            {
                cleanupDb.ImportJobs.RemoveRange(toDelete);
                await cleanupDb.SaveChangesAsync();
            }
        }

        GC.SuppressFinalize(this);
    }
}
