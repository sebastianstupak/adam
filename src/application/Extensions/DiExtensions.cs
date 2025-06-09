using ADAM.Application.Bot;
using ADAM.Application.Bot.Commands;
using ADAM.Application.Jobs;
using ADAM.Application.Services;
using ADAM.Application.Services.Users;
using ADAM.Application.Sites;
using ADAM.Domain.Repositories.Subscriptions;
using ADAM.Domain.Repositories.Users;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace ADAM.Application.Extensions;

public static class DiExtensions
{
    public static IServiceCollection AddAdamServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    public static IServiceCollection AddBotFramework(this IServiceCollection services)
    {
        services.AddScoped<ICommand, ConsentCommand>();
        services.AddScoped<ICommand, DataCommand>();
        services.AddScoped<ICommand, CreateSubscriptionCommand>();
        services.AddScoped<ICommand, UpdateSubscriptionCommand>();
        services.AddScoped<ICommand, DeleteSubscriptionCommand>();
        services.AddScoped<ICommand, ListSubscriptionsCommand>();
        services.AddScoped<ICommand, HereCommand>();
        services.AddScoped<ICommand, HelpCommand>();

        services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
        services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

        services.AddScoped<MessageSendingService>();

        services.AddTransient<IBot, AdamBot>();

        return services;
    }

    /// <summary>
    /// Provides the <see cref="IServiceCollection"/> with the implementations of sites ADAM is capable of scalping.
    /// </summary>
    public static IServiceCollection AddSites(this IServiceCollection services)
    {
        services.AddScoped<IMerchantSite, AuparkSite>();
        services.AddScoped<IMerchantSite, LittleIndiaSite>();
        services.AddScoped<IJob, ScrapeAndNotifyJob>();
        return services;
    }
}