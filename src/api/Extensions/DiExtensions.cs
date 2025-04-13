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

    public static IServiceCollection AddAdamServices(this IServiceCollection services)
    {
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }

    public static WebApplicationBuilder AddDbContext(this WebApplicationBuilder builder,
        string? connectionString = null)
    {
        connectionString ??= builder.Configuration.GetConnectionString("DefaultConnection") ??
                             throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddDbContext<AppDbContext>(opts => opts.UseNpgsql(connectionString));

        builder.Services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "database")
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

        return builder;
    }
}