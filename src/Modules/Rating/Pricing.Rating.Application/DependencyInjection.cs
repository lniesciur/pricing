using Microsoft.Extensions.DependencyInjection;

namespace Pricing.Rating.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddRatingApplication(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(classes => classes.Where(t => t.Name.EndsWith("UseCase")))
            .AsSelf()
            .WithScopedLifetime()
            .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Facade")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
