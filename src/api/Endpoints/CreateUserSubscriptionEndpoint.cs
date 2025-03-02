using ADAM.Application.Objects;
using ADAM.Application.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace ADAM.API.Endpoints;

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
        catch (UserNotFoundException)
        {
            return Results.NotFound($"A user with GUID '{dto.UserGuid}' does not exist");
        }
        catch (ArgumentOutOfRangeException e)
        {
            return Results.BadRequest(e.Message);
        }
    }
}