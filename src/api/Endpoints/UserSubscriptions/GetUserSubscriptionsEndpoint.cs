using ADAM.Application.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace ADAM.API.Endpoints.UserSubscriptions;

public class GetUserSubscriptionsEndpoint
{
    public static async Task<IResult> HandleAsync([FromRoute] string teamsId, [FromServices] IUserService userService)
    {
        var subscriptions = await userService.GetUserSubscriptionsAsync(teamsId);
        return Results.Ok(subscriptions);
    }
}