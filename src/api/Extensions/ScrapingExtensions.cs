using ADAM.Application.Sites;

namespace ADAM.API.Extensions;

public static class ScrapingExtensions
{
    /// <summary>
    /// Provides the <see cref="IServiceCollection"/> with the implementations of sites ADAM is capable of scalping.
    /// </summary>
    public static IServiceCollection AddSites(this IServiceCollection services)
    {
        services.AddScoped<IMerchantSite, AuparkSite>();
        return services;
    }
}