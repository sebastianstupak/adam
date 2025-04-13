using ADAM.Application.Objects;
using ADAM.Application.Services.Users;
using ADAM.Domain.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace ADAM.Bot;

public class AdamBot(IUserService userService) : ActivityHandler
{
    private readonly IUserService _userService = userService;

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
        CancellationToken cancellationToken)
    {
        var messageContent = turnContext.Activity.Text;
        var userId = turnContext.Activity.From.Id
                     ?? throw new Exception("Unable to get user ID from interaction!");

        if (!messageContent.StartsWith(CommandConstants.AdamBase, StringComparison.InvariantCultureIgnoreCase))
            return;

        var parts = messageContent.Split(" ");
        if (!parts[0].Equals(CommandConstants.AdamBase, StringComparison.InvariantCultureIgnoreCase))
            return;

        if (parts.Length < 2)
            return;

        if (StringSubstringComparison(parts[1], CommandConstants.Subscribe, [0, 3, 0, 1]))
        {
            switch (parts[2])
            {
                case CommandConstants.List:
                {
                    var subscriptions = (await _userService.GetUserSubscriptionsAsync(userId)).ToList();

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

                    break;
                }
                case CommandConstants.Company:
                {
                    try
                    {
                        await _userService.CreateUserSubscriptionAsync(new CreateUserSubscriptionDto
                        {
                            TeamsId = userId,
                            Type = SubscriptionType.Merchant,
                            Value = string.Join(" ", parts[3..])
                        });

                        await turnContext.SendActivityAsync(
                            MessageFactory.Text("Subscription created successfully."), cancellationToken
                        );
                    }
                    catch (Exception e)
                    {
                        await turnContext.SendActivityAsync(
                            MessageFactory.Text($"Error creating subscription: {e.Message}"), cancellationToken
                        );
                    }

                    break;
                }
                case CommandConstants.Food:
                {
                    try
                    {
                        await _userService.CreateUserSubscriptionAsync(new CreateUserSubscriptionDto
                        {
                            TeamsId = userId,
                            Type = SubscriptionType.Offer,
                            Value = string.Join(" ", parts[3..])
                        });

                        await turnContext.SendActivityAsync(
                            MessageFactory.Text("Subscription created successfully."), cancellationToken
                        );
                    }
                    catch (Exception e)
                    {
                        await turnContext.SendActivityAsync(
                            MessageFactory.Text($"Error creating subscription: {e.Message}"), cancellationToken
                        );
                    }

                    break;
                }
                case CommandConstants.Update:
                {
                    try
                    {
                        if (!int.TryParse(parts[3], out var subscriptionId))
                            throw new Exception("Subscription ID must be an integer.");

                        await _userService.UpdateUserSubscriptionAsync(
                            subscriptionId,
                            new UpdateUserSubscriptionDto
                            {
                                NewValue = string.Join(" ", parts[4..])
                            },
                            userId
                        );

                        await turnContext.SendActivityAsync(
                            MessageFactory.Text("Subscription created successfully."), cancellationToken
                        );
                    }
                    catch (Exception e)
                    {
                        await turnContext.SendActivityAsync(
                            MessageFactory.Text($"Error creating subscription: {e.Message}"), cancellationToken
                        );
                    }

                    break;
                }
            }
        }

        if (StringSubstringComparison(parts[1], CommandConstants.Unsubscribe, [0, 5, 0, 3]))
        {
        }
    }

    private static bool StringSubstringComparison(string testee, string target, int[] substringLimits)
    {
        var match = false;

        if (substringLimits.Length % 2 != 0)
            throw new ArgumentException($"{nameof(substringLimits)} must have an even count of values.");

        var substringLimitPairs = new List<(int, int)>();
        for (var i = 0; i < substringLimits.Length; i += 2)
        {
            if (i + 1 < substringLimits.Length)
                substringLimitPairs.Add((substringLimits[i], substringLimits[i + 1]));
        }

        foreach (var pair in substringLimitPairs)
        {
            if (testee.Equals(target, StringComparison.InvariantCultureIgnoreCase)
                || testee.Equals(target.Substring(pair.Item1, pair.Item2), StringComparison.InvariantCultureIgnoreCase))
            {
                match = true;
                break;
            }
        }

        return match;
    }
}