using ADAM.Application.Objects;
using ADAM.Application.Services.Users;
using ADAM.Domain.Models;
using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public class CreateSubscriptionCommand(IUserService userService) : ICommand
{
    private readonly IUserService _userService = userService;

    public async Task HandleAsync(ITurnContext turnContext, string[] commandParts, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.CreateUserSubscriptionAsync(new CreateUserSubscriptionDto
            {
                TeamsId = turnContext.Activity.From.Id,
                Type = SubscriptionType.Merchant,
                Value = string.Join(" ", commandParts[3..])
            });

            await turnContext.SendActivityAsync(
                MessageFactory.Text("✅ Subscription created successfully."), cancellationToken
            );
        }
        catch (Exception e)
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text($"❌ Error creating subscription: {e.Message}"), cancellationToken
            );
        }
    }
}