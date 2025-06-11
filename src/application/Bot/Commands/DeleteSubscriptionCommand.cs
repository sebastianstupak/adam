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
            var teamsId = context.Activity.From.Id;

            if (!int.TryParse(cmdParts.ElementAtOrDefault(2), out var subscriptionId))
                throw new Exception("Missing or malformed subscription ID!");

            await _userService.DeleteUserSubscriptionAsync(subscriptionId, teamsId);

            await context.SendActivityAsync(
                MessageFactory.Text("✅ Subscription deleted successfully."), ct
            );
        }
        catch (Exception ex)
        {
            await context.SendActivityAsync(
                MessageFactory.Text($"❌ Error deleting a subscription: {ex.Message}"), ct
            );
        }
    }

    public override string GetCommandName() => "Delete Subscription";
    public override string GetCommandUsageExample() => "@adam uns (id)";
    public override string GetCommandDescription() => "Used to unsubscribe from a food or company alert.";
}