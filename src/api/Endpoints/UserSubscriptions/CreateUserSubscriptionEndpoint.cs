using ADAM.Application.Objects;
using ADAM.Application.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace ADAM.API.Endpoints.UserSubscriptions;

public class CreateUserSubscriptionEndpoint
{
    public static async Task<IResult> HandleAsync([FromBody] CreateUserSubscriptionDto dto,
        [FromServices] IUserService userService)
    {
        try
        {
            await userService.CreateUserSubscriptionAsync(dto);
            return Results.Created();
        }
        catch (ArgumentOutOfRangeException e)
        {
            return Results.BadRequest(e.Message);
        }
    }
}