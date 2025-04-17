using ADAM.Application.Objects;
using ADAM.Application.Services.Users;
using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public class UpdateSubscriptionCommand(IUserService userService) : ICommand
{
    private readonly IUserService _userService = userService;

    public async Task HandleAsync(ITurnContext turnContext, string[] commandParts, CancellationToken cancellationToken)
    {
        try
        {
            if (!int.TryParse(commandParts.ElementAtOrDefault(3), out var subscriptionId))
                throw new Exception("Missing or malformed subscription ID!");

            await _userService.UpdateUserSubscriptionAsync(
                subscriptionId,
                new UpdateUserSubscriptionDto
                {
                    NewValue = string.Join(" ", commandParts[4..])
                },
                turnContext.Activity.From.Id
            );

            await turnContext.SendActivityAsync(
                MessageFactory.Text("✅ Subscription updated successfully."), cancellationToken
            );
        }
        catch
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("❌ Error updating subscription"), cancellationToken
            );
        }
    }
}