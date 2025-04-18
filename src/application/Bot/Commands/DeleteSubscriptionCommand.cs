using ADAM.Application.Services.Users;
using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public class DeleteSubscriptionCommand(IUserService userService) : Command
{
    private readonly IUserService _userService = userService;

    protected override async Task HandleCommandAsync(ITurnContext context, string[] cmdParts, CancellationToken ct)
    {
        try
        {
            if (!int.TryParse(cmdParts.ElementAtOrDefault(2), out var subscriptionId))
                throw new Exception("Missing or malformed subscription ID!");

            await _userService.DeleteUserSubscriptionAsync(subscriptionId);

            await context.SendActivityAsync(
                MessageFactory.Text("✅ Subscription deleted successfully."), ct
            );
        }
        catch
        {
            await context.SendActivityAsync(
                MessageFactory.Text($"❌ Error deleting a subscription."), ct
            );
        }
    }
}