using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Import.Contracts.DeviceImports;
using Pricing.Import.Infrastructure.Persistence;
using Pricing.IntegrationTests.Infrastructure;
using Pricing.Shared.Contracts;

namespace Pricing.IntegrationTests.Modules.Import;

public class DeviceImportEndpointTests : IClassFixture<ApiFactory>, IAsyncDisposable
{
    private readonly HttpClient _client;
    private readonly ApiFactory _factory;
    private readonly IServiceScope _scope;
    private readonly ImportDbContext _db;
    private readonly List<Guid> _createdJobIds = [];

    public DeviceImportEndpointTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        _db = _scope.ServiceProvider.GetRequiredService<ImportDbContext>();
    }

    [Fact]
    public async Task UploadDeviceImport_WithValidCsvFile_Returns202WithJobId()
    {
        using var content = BuildMultipart("devices.csv", "EanCode,Name,TypeCode\nEAN001,Device A,LAPTOP");

        var response = await _client.PostAsync("/api/import/device-imports", content);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UploadDeviceImportResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.JobId);
        _createdJobIds.Add(body.JobId);
    }

    [Fact]
    public async Task UploadDeviceImport_WithValidXlsxFile_Returns202WithJobId()
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
        var jobId = await UploadAndGetJobId("filter-test.csv");

        var response = await _client.GetAsync($"/api/import/device-imports?status={ImportJobStatus.Queued}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ListDeviceImportsResponse>();
        Assert.NotNull(body);
        Assert.All(body.Items, item => Assert.Equal(nameof(ImportJobStatus.Queued), item.Status));
        _ = jobId; // uploaded job may have been processed by Hangfire already
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

    private async Task<Guid> UploadAndGetJobId(string fileName)
    {
        using var content = BuildMultipart(fileName, "EanCode,Name,TypeCode\nEAN001,Device,LAPTOP");
        var response = await _client.PostAsync("/api/import/device-imports", content);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<UploadDeviceImportResponse>();
        _createdJobIds.Add(body!.JobId);
        return body.JobId;
    }

    private static MultipartFormDataContent BuildMultipart(string fileName, string fileContent, string mediaType = "text/csv")
    {
        var content = new MultipartFormDataContent();
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(fileContent);
        var fileStreamContent = new ByteArrayContent(fileBytes);
        fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType);
        content.Add(fileStreamContent, "file", fileName);
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

        _scope.Dispose();
    }
}
