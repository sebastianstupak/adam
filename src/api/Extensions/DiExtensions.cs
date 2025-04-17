using ADAM.Application.Services.Users;
using ADAM.Application.Sites;
using ADAM.Domain;
using ADAM.Domain.Repositories.Subscriptions;
using ADAM.Domain.Repositories.Users;
using Microsoft.EntityFrameworkCore;

namespace ADAM.API.Extensions;

public static class DiExtensions
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