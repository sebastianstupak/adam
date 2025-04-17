using ADAM.Application.Services.Users;
using ADAM.Domain.Repositories.Subscriptions;
using ADAM.Domain.Repositories.Users;
using Microsoft.Extensions.DependencyInjection;

namespace ADAM.Application.Extensions;

public static class DiExtensions
{
    public static IServiceCollection AddAdamServices(this IServiceCollection services)
    {
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}