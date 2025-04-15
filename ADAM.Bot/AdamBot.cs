using ADAM.Application.Objects;
using ADAM.Application.Services.Users;
using ADAM.Domain;
using ADAM.Domain.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Bot;

public class AdamBot(IUserService userService, AppDbContext dbCtx, MessageSender msgSender) : ActivityHandler
{
    private readonly IUserService _userService = userService;
    private readonly AppDbContext _dbCtx = dbCtx;
    private readonly MessageSender _msgSender = msgSender; // TODO: Temp, remove once done testing

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
        CancellationToken cancellationToken)
    {
        var messageContent = turnContext.Activity.Text;

        var teamsId = turnContext.Activity.From.Id
                      ?? throw new Exception("Unable to get user ID from interaction!");

        if (!messageContent.StartsWith(CommandConstants.AdamBase, StringComparison.InvariantCultureIgnoreCase))
            return;

        var parts = messageContent.Split(" ");
        if (!parts[0].Equals(CommandConstants.AdamBase, StringComparison.InvariantCultureIgnoreCase))
            return;

        if (!await _userService.DidUserAcceptDataStorageAsync(teamsId)
            && !StringMatchesFullOrSubString(parts.ElementAtOrDefault(1) ?? "", CommandConstants.Consent))
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("""
                                    Hello, this might be our first interaction!

                                    To use my services, please consent to your data being stored by typing `@adam consent`.
                                    To learn what data we store about you, type `@adam data`.
                                    """),
                cancellationToken
            );

            return;
        }

        if (parts.Length < 2)
            return;

        if (!StringMatchesFullOrSubString(parts.ElementAtOrDefault(1) ?? "", CommandConstants.Consent))
            await PersistConversationReferenceAsync(teamsId, turnContext);

        // TODO: Temp, remove once done testing
        // if (messageContent == "@adam test")
        // {
        //     await _msgSender.SendMessageToUserAsync(
        //         teamsId,
        //         "This is a proactive message",
        //         cancellationToken
        //     );
        // }

        if (StringMatchesFullOrSubString(parts[1], CommandConstants.Subscribe, [0, 3, 0, 1]))
        {
            switch (parts[2])
            {
                case CommandConstants.List:
                {
                    var subscriptions = (await _userService.GetUserSubscriptionsAsync(teamsId)).ToList();

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
                            TeamsId = teamsId,
                            Type = SubscriptionType.Merchant,
                            Value = string.Join(" ", parts[3..])
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

                    break;
                }

                case CommandConstants.Food:
                {
                    try
                    {
                        await _userService.CreateUserSubscriptionAsync(new CreateUserSubscriptionDto
                        {
                            TeamsId = teamsId,
                            Type = SubscriptionType.Offer,
                            Value = string.Join(" ", parts[3..])
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

                    break;
                }

                case CommandConstants.Update:
                {
                    try
                    {
                        if (!int.TryParse(parts.ElementAtOrDefault(3), out var subscriptionId))
                            throw new Exception("Missing or malformed subscription ID!");

                        await _userService.UpdateUserSubscriptionAsync(
                            subscriptionId,
                            new UpdateUserSubscriptionDto
                            {
                                NewValue = string.Join(" ", parts[4..])
                            },
                            teamsId
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

                    break;
                }
            }
        }

        if (StringMatchesFullOrSubString(parts[1], CommandConstants.Unsubscribe, [0, 5, 0, 3]))
        {
            try
            {
                if (!int.TryParse(parts.ElementAtOrDefault(2), out var subscriptionId))
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

        if (StringMatchesFullOrSubString(parts[1], CommandConstants.Consent))
        {
            try
            {
                await _userService.UpdateUserConsentAsync(teamsId);
                await turnContext.SendActivityAsync(
                    MessageFactory.Text("✅ Consent updated."), cancellationToken
                );
            }
            catch
            {
                await turnContext.SendActivityAsync(
                    MessageFactory.Text("❌ Error updating consent!"), cancellationToken
                );
            }
        }
    }

    private static bool StringMatchesFullOrSubString(string testee, string target, int[]? substringLimits = null)
    {
        var match = false;

        if (substringLimits is null)
            return testee.Equals(target, StringComparison.InvariantCultureIgnoreCase);

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

    private async Task PersistConversationReferenceAsync(string teamsId, ITurnContext turnContext)
    {
        var convRef = turnContext.Activity.GetConversationReference();
        ArgumentNullException.ThrowIfNull(convRef);

        var user = await _dbCtx.Users.FirstOrDefaultAsync(u =>
            u.TeamsId == teamsId
        );
        ArgumentNullException.ThrowIfNull(user);

        var crExists = await _dbCtx.ConversationReferences.AnyAsync(cr => cr.UserId == user.Id);

        if (crExists)
        {
            var crToUpdate = await _dbCtx.ConversationReferences
                .Where(cr => cr.UserId == user.Id)
                .FirstAsync();

            crToUpdate.ActivityId = convRef.ActivityId;
            crToUpdate.ChannelId = convRef.ChannelId;
            crToUpdate.ServiceUrl = convRef.ServiceUrl;
        }
        else
        {
            _dbCtx.ConversationReferences.Add(
                new Domain.Models.ConversationReference
                {
                    UserId = user.Id,
                    ActivityId = convRef.ActivityId,
                    ChannelId = convRef.ChannelId,
                    ServiceUrl = convRef.ServiceUrl
                }
            );

            await _dbCtx.SaveChangesAsync();
        }
    }
}