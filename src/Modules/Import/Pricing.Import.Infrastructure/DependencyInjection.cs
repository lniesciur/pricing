using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Import.Application;
using Pricing.Import.Application.FileReading;
using Pricing.Import.Infrastructure.FileReading;

namespace Pricing.Import.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddImportInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Scan(scan => scan
            .FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Repository")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddScoped<CsvFileReader>();
        services.AddScoped<ExcelFileReader>();
        services.AddScoped<IFileReader, FileReaderFacade>();

        services.AddImportApplication();

        return services;
    }
}
