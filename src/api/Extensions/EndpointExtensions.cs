using ADAM.API.Endpoints;
using ADAM.Application.Services.User;

namespace ADAM.API.Extensions;

public static class EndpointExtensions
{
    public static void RegisterUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var users = routes.MapGroup("/api/v1/users");

        users.MapGet("/{guid:guid}", GetUserSubscriptionsEndpoint.HandleAsync);
    }
}