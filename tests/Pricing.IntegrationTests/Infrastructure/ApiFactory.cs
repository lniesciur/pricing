using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Import.Application;
using Testcontainers.MsSql;

namespace Pricing.IntegrationTests.Infrastructure;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _mssql = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public async Task InitializeAsync()
    {
        await _mssql.StartAsync();
        _ = Services; // trigger host startup: migration + seeding run via StartInventoryModuleAsync
    }

    public new async Task DisposeAsync()
    {
        await _mssql.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _mssql.GetConnectionString(),
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace Hangfire scheduler with a synchronous version so that every upload
            // immediately processes the job — no polling or race conditions in tests.
            var descriptor = services.Single(d => d.ServiceType == typeof(IImportJobScheduler));
            services.Remove(descriptor);
            services.AddScoped<IImportJobScheduler, SynchronousImportJobScheduler>();
        });
    }
}
