using ADAM.Application.Objects;
using ADAM.Application.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace ADAM.API.Endpoints;

public class DeleteUserSubscriptionEndpoint
{
    public static async Task<IResult> HandleAsync([FromRoute] int id, [FromServices] IUserService userService)
    {
        try
        {
            await userService.DeleteUserSubscriptionAsync(id);
            return Results.Ok();
        }
        catch (SubscriptionNotFoundException)
        {
            return Results.NotFound($"A subscription with ID '{id} does not exist'");
        }
    }
}