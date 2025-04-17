using ADAM.Application.Services.Users;
using ADAM.Domain.Models;
using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public class ListSubscriptionsCommands(IUserService userService) : ICommand
{
    private readonly IUserService _userService = userService;

    public async Task HandleAsync(ITurnContext turnContext, string[] commandParts, CancellationToken cancellationToken)
    {
        var subscriptions = (await _userService.GetUserSubscriptionsAsync(turnContext.Activity.From.Id)).ToList();

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

        await turnContext.SendActivityAsync(output, cancellationToken);
    }
}