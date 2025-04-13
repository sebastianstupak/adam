using ADAM.Application.Objects;
using ADAM.Application.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace ADAM.API.Endpoints;

public class UpdateUserSusbcriptionEndpoint
{
    public static async Task<IResult> HandleAsync([FromRoute] int id, [FromBody] UpdateUserSubscriptionDto dto, [FromServices] IUserService userService)
    {
        try
        {
            await userService.UpdateUserSubscriptionAsync(id, dto, null); // TODO: Decide
            return Results.Created();
        }
        catch (SubscriptionNotFoundException)
        {
            return Results.NotFound($"A subscription with ID '{id} does not exist'");
        }
        catch (ArgumentOutOfRangeException e)
        {
            return Results.BadRequest(e.Message);
        }
    }
}