using ADAM.Application.Objects;
using ADAM.Application.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace ADAM.API.Endpoints.UserSubscriptions;

public class DeleteUserSubscriptionEndpoint
{
    public static async Task<IResult> HandleAsync([FromBody] DeleteUserSubscriptionDto dto,
        [FromServices] IUserService userService)
    {
        try
        {
            await userService.DeleteUserSubscriptionAsync(dto.Id, dto.TeamsId);
            return Results.Ok();
        }
        catch (SubscriptionNotFoundException)
        {
            return Results.NotFound($"A subscription with ID '{dto.Id} does not exist'");
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }
}