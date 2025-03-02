using ADAM.Application.Services.User;
using ADAM.Domain.Models;

namespace ADAM.API.Endpoints;

public class GetUserSubscriptionsEndpoint
{
    public static async Task<IResult> HandleAsync(Guid guid, IUserService userService)
    {
        var subscriptions = await userService.GetUserSubscriptionsAsync(guid);
        var response = subscriptions.Select(MapUserSubscriptionEntityToResponse);
        return Results.Ok(response);
    }
    
    private static GetUserSubscriptionsResponse MapUserSubscriptionEntityToResponse(Subscription subscription)
    {
        return new GetUserSubscriptionsResponse
        {
            Id = subscription.Id,
            Value = subscription.Value,
            Type = subscription.Type,
        };
    }
}

public record GetUserSubscriptionsResponse
{
    public required long Id { get; set; }
    
    public required SubscriptionType Type { get; set; }
    
    public required string Value { get; set; }
}