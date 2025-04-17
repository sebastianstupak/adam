using ADAM.Application.Services.Users;
using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public class DeleteSubscriptionCommand(IUserService userService) : ICommand
{
    private readonly IUserService _userService = userService;

    public async Task HandleAsync(ITurnContext turnContext, string[] commandParts, CancellationToken cancellationToken)
    {
        try
        {
            if (!int.TryParse(commandParts.ElementAtOrDefault(2), out var subscriptionId))
                throw new Exception("Missing or malformed subscription ID!");

            await _userService.DeleteUserSubscriptionAsync(subscriptionId);

            await turnContext.SendActivityAsync(
                MessageFactory.Text("✅ Subscription deleted successfully."), cancellationToken
            );
        }
        catch
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("❌ Error deleting subscription"), cancellationToken
            );
        }
    }
}