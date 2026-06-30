using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Import.Infrastructure;
using Pricing.Import.Infrastructure.Persistence;

namespace Pricing.Import.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddImportModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddImportInfrastructure(configuration);
        return services;
    }

    public static async Task StartImportModuleAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ImportDbContext>();
        await db.Database.MigrateAsync();
    }
}
