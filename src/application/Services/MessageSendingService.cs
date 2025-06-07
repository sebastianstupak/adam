using System.Collections.Concurrent;
using System.Text;
using ADAM.Application.Objects;
using ADAM.Domain;
using ADAM.Domain.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using ConversationReference = Microsoft.Bot.Schema.ConversationReference;

namespace ADAM.Application.Services;

public class MessageSendingService(IBotFrameworkHttpAdapter adapter, AppDbContext dbCtx)
{
    private readonly IBotFrameworkHttpAdapter _adapter = adapter;
    private readonly AppDbContext _dbCtx = dbCtx;

    public async Task SendCombinedNotificationAsync(
        string botId,
        IEnumerable<UserSubscriptions> mealSubs,
        IEnumerable<UserSubscriptions> merchantSubs,
        ConcurrentBag<MerchantOffer> merchantOffers,
        CancellationToken cancellationToken = default)
    {
        var convRef = await _dbCtx.ConversationReferences.FirstOrDefaultAsync(cancellationToken: cancellationToken);
        ArgumentNullException.ThrowIfNull(convRef);

        var conversationReference = new ConversationReference
        {
            Bot = new ChannelAccount { Id = botId },
            Conversation = new ConversationAccount { Id = convRef.ConversationId },
            ServiceUrl = convRef.ServiceUrl,
            ChannelId = "msteams"
        };

        await ((CloudAdapter)_adapter).ContinueConversationAsync(
            botId,
            conversationReference,
            async (turnContext, ct) =>
            {
                var combinedMessage = CreateCombinedMessage(mealSubs, merchantSubs, merchantOffers);
                await turnContext.SendActivityAsync(combinedMessage, cancellationToken: ct);
            },
            cancellationToken
        );
    }

    private Activity CreateCombinedMessage(
        IEnumerable<UserSubscriptions> mealSubs,
        IEnumerable<UserSubscriptions> merchantSubs,
        IEnumerable<MerchantOffer> merchantOffers)
    {
        var allUserSubs = mealSubs.Concat(merchantSubs);

        var offerUserGroups = merchantOffers
            .Select(offer => new
            {
                Offer = offer,
                MatchingUsers = allUserSubs
                    .SelectMany(userSub => userSub.Subscriptions
                        .Where(sub => DoesOfferMatchSubscription(offer, sub))
                        .Select(_ => userSub.User))
                    .Distinct()
                    .ToList()
            })
            .Where(x => x.MatchingUsers.Any())
            .ToList();

        var messageBuilder = new StringBuilder();
        var mentionEntities = new List<Entity>();

        foreach (var group in offerUserGroups)
        {
            messageBuilder.AppendLine($"**{group.Offer.Meal}** ({group.Offer.MerchantName})");
            messageBuilder.AppendLine($"Price: {(group.Offer.Price is null
                    ? "unknown"
                    : $"{group.Offer.Price.ToString()} €"
                )}");

            var userMentions = new List<string>();

            foreach (var user in group.MatchingUsers)
            {
                var mentionText = $"<at>{user.Name}</at>";
                userMentions.Add(mentionText);

                mentionEntities.Add(new Mention
                {
                    Mentioned = new ChannelAccount
                    {
                        Id = user.TeamsId,
                        Name = user.Name
                    },
                    Text = mentionText
                });
            }

            messageBuilder.AppendLine(string.Join(", ", userMentions));
            messageBuilder.AppendLine();
        }

        var activity = MessageFactory.Text(messageBuilder.ToString().Trim());
        activity.Entities = mentionEntities;

        return activity;
    }

    private bool DoesOfferMatchSubscription(MerchantOffer offer, Subscription subscription)
    {
        return subscription.Type switch
        {
            SubscriptionType.Merchant => offer.MerchantName.Contains(subscription.Value,
                StringComparison.InvariantCultureIgnoreCase),
            SubscriptionType.Offer => offer.Meal.Contains(subscription.Value,
                StringComparison.InvariantCultureIgnoreCase),
            _ => false
        };
    }
}