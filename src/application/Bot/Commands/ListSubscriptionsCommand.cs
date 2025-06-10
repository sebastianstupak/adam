using ADAM.Application.Objects;
using ADAM.Application.Services.Users;
using ADAM.Domain.Models;
using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public class ListSubscriptionsCommand(IUserService userService) : Command
{
    private readonly IUserService _userService = userService;

    protected override async Task HandleCommandAsync(ITurnContext context, string[] cmdParts, CancellationToken ct)
    {
        var subscriptions =
            (await _userService.GetUserSubscriptionsAsync(context.Activity.From.Id))
            .ToList();

        var output = subscriptions.Count != 0
            ? MessageFactory.Text(
                "*(id) item*\n\n" +
                $"# Companies:\n{
                    string.Join(
                        ", ",
                        subscriptions.Where(s => s.Type == SubscriptionType.Merchant).Select(s => $"({s.Id}) {s.Value}")
                    )
                }\n" +
                $"# Food:\n{
                    string.Join(
                        ", ",
                        subscriptions.Where(s => s.Type == SubscriptionType.Offer).Select(s => $"({s.Id}) {s.Value}")
                    )
                }"
            )
            : MessageFactory.Text("No subscriptions found.");

        await context.SendActivityAsync(output, ct);
    }

    public override string GetCommandName() => "List Subscriptions";
    public override string GetCommandUsageExample() => "@adam list";
    public override string GetCommandDescription() => "Lists all your subscriptions.";

    public override CommandMatchTargets GetCommandMatchTargets() => new()
    {
        CommandTargets = [CommandConstants.Subscribe, CommandConstants.Subscribe[..3], CommandConstants.Subscribe[..1]],
        SubcommandTargets = [CommandConstants.List]
    };
}