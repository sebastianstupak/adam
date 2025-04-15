using ADAM.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Bot;

public class MessageSender(
    IBotFrameworkHttpAdapter adapter,
    IConfiguration configuration,
    AppDbContext dbCtx
)
{
    private readonly AppDbContext _dbCtx = dbCtx;
    private readonly string _botId = configuration["BotId"] ?? "abcdefgh";

    // Send a message to a specific user by their ID
    public async Task SendMessageToUserAsync(string teamsId, string message,
        CancellationToken cancellationToken = default)
    {
        var convRef = await _dbCtx.ConversationReferences.FirstOrDefaultAsync(
            cr => cr.User.TeamsId == teamsId, cancellationToken: cancellationToken
        );
        ArgumentNullException.ThrowIfNull(convRef);

        var conversationReference = new ConversationReference
        {
            ChannelId = convRef.ChannelId,
            ServiceUrl = convRef.ServiceUrl,
            ActivityId = convRef.ActivityId
            // User = new ChannelAccount { Id = userId },
            // Bot = _turnContext.Activity.Recipient
        };

        await ((CloudAdapter)adapter).ContinueConversationAsync(
            _botId,
            conversationReference,
            async (turnContext, ct) =>
            {
                // Send the message
                await turnContext.SendActivityAsync(message, cancellationToken: ct);
            },
            cancellationToken
        );
    }
}