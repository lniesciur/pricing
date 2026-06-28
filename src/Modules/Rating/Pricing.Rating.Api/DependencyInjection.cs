using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Rating.Infrastructure;

namespace Pricing.Rating.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddRatingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRatingInfrastructure(configuration);
        return services;
    }
}
