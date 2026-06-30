using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Import.Application;
using Pricing.Import.Application.FileReading;
using Pricing.Import.Application.UseCases.ProcessDeviceImport;
using Pricing.Import.Infrastructure.FileReading;
using Pricing.Import.Infrastructure.Persistence;

namespace Pricing.Import.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddImportInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ImportDbContext>(opts =>
            opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.Scan(scan => scan
            .FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Repository")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddScoped<IImportUnitOfWork, ImportUnitOfWork>();
        services.AddScoped<IImportJobScheduler, HangfireImportJobScheduler>();
        services.AddScoped<ProcessDeviceImportUseCase>();

        services.AddScoped<CsvFileReader>();
        services.AddScoped<ExcelFileReader>();
        services.AddScoped<IFileReader, FileReaderFacade>();

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));

        services.AddHangfireServer();

        services.AddImportApplication();

        return services;
    }
}
