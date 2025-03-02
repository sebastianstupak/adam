using ADAM.API.Endpoints;
using ADAM.Application.Objects;
using ADAM.Application.Services.Users;

namespace ADAM.API.Extensions;

public static class EndpointExtensions
{
    public static void RegisterAdamEndpoints(this IEndpointRouteBuilder endpoints)
    {
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

        users.MapGet("{guid:guid}/subscriptions", GetUserSubscriptionsEndpoint.HandleAsync)
            .Produces<IEnumerable<GetUserSubscriptionDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}