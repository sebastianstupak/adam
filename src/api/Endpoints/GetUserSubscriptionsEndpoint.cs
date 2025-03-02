using ADAM.Application.Services.Users;
using ADAM.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace ADAM.API.Endpoints;

public class GetUserSubscriptionsEndpoint
{
    public static async Task<IResult> HandleAsync([FromRoute] Guid guid, [FromServices] IUserService userService)
    {
        try
        {
            var subscriptions = await userService.GetUserSubscriptionsAsync(guid);
            return Results.Ok(subscriptions);
        }
        catch (UserNotFoundException)
        {
            return Results.NotFound($"A user with GUID '{guid}' does not exist");
        }
    }
}