using ADAM.Application.Objects;
using ADAM.Application.Services.Users;
using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public class UpdateSubscriptionCommand(IUserService userService) : Command
{
    private readonly IUserService _userService = userService;

    protected override async Task HandleCommandAsync(ITurnContext context, string[] cmdParts, CancellationToken ct)
    {
        try
        {
            if (!int.TryParse(cmdParts.ElementAtOrDefault(3), out var subscriptionId))
                throw new Exception("Missing or malformed subscription ID!");

            await _userService.UpdateUserSubscriptionAsync(
                subscriptionId,
                new UpdateUserSubscriptionDto
                {
                    NewValue = string.Join(" ", cmdParts[4..])
                },
                context.Activity.From.Id
            );

            await context.SendActivityAsync(
                MessageFactory.Text("✅ Subscription updated successfully."), ct
            );
        }
        catch
        {
            await context.SendActivityAsync(
                MessageFactory.Text("❌ Error updating subscription"), ct
            );
        }
    }

    public override string GetCommandName() => "Update Subscription";
    public override string GetCommandUsageExample() => "@adam s update (id) (new value)";
    public override string GetCommandDescription() => "Used to update the value of a subscription.";

    public override CommandMatchTargets GetCommandMatchTargets() => new()
    {
        CommandTargets = [CommandConstants.Subscribe, CommandConstants.Subscribe[..3], CommandConstants.Subscribe[..1]],
        SubcommandTargets = [CommandConstants.Update]
    };
}