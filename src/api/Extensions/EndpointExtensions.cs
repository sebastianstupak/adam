using ADAM.API.Endpoints.UserSubscriptions;
using ADAM.Application.Jobs;
using ADAM.Application.Objects;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace ADAM.API.Extensions;

public static class EndpointExtensions
{
    public static void RegisterAdamEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.RegisterBotEndpoints();
        endpoints.RegisterSubscriptionEndpoints();
        endpoints.RegisterUserEndpoints();
    }

    private static void RegisterSubscriptionEndpoints(this IEndpointRouteBuilder routes)
    {
        var subscription = routes.MapGroup("/api/v1/subscription");
        var subscriptions = routes.MapGroup("api/v1/subscriptions");

        subscription.MapPost("", CreateUserSubscriptionEndpoint.HandleAsync)
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        subscriptions.MapPut("{id:int}", UpdateUserSusbcriptionEndpoint.HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        subscriptions.MapDelete("{id:int}", DeleteUserSubscriptionEndpoint.HandleAsync)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static void RegisterUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var users = routes.MapGroup("/api/v1/user");

        users.MapGet("{teamsId:guid}/subscriptions", GetUserSubscriptionsEndpoint.HandleAsync)
            .Produces<IEnumerable<GetUserSubscriptionDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static void RegisterBotEndpoints(this IEndpointRouteBuilder routes)
    {
        var api = routes.MapGroup("/api").ExcludeFromDescription();

        api.MapPost("messages",
            async (HttpRequest request, HttpResponse response, IBotFrameworkHttpAdapter adapter, IBot bot) =>
            {
                await adapter.ProcessAsync(request, response, bot);
            }
        );

        api.MapGet("messages",
            async (HttpRequest request, HttpResponse response, IBotFrameworkHttpAdapter adapter, IBot bot) =>
            {
                await adapter.ProcessAsync(request, response, bot);
            }
        );
    }

    // TODO: For testing
    // private static void RegisterTestingEndpoints(this IEndpointRouteBuilder routes)
    // {
    //     var testing = routes.MapGroup("/api/v1/testing");
    //     testing.MapGet("job", ([FromServices] IJob job) => job.ExecuteAsync());
    // }
}