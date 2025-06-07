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
                "*item (id)*\n\n" +
                $"# Companies:\n{
                    string.Join(
                        ", ",
                        subscriptions.Where(s => s.Type == SubscriptionType.Merchant).Select(s => $"{s.Value} ({s.Id})")
                    )
                }\n" +
                $"# Food:\n{
                    string.Join(
                        ", ",
                        subscriptions.Where(s => s.Type == SubscriptionType.Offer).Select(s => $"{s.Value} ({s.Id})")
                    )
                }"
            )
            : MessageFactory.Text("No subscriptions found.");

        await context.SendActivityAsync(output, ct);
    }
}