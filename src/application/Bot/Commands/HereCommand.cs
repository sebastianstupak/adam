using ADAM.Domain;
using ADAM.Domain.Models;
using Microsoft.Bot.Builder;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Application.Bot.Commands;

public class HereCommand(AppDbContext dbCtx) : Command
{
    private readonly AppDbContext _dbCtx = dbCtx;

    protected override async Task HandleCommandAsync(ITurnContext context, string[] cmdParts, CancellationToken ct)
    {
        try
        {
            // convRef -> conversation reference
            var convRef = context.Activity.GetConversationReference();
            ArgumentNullException.ThrowIfNull(convRef);

            var globalChannelSetting = await _dbCtx.ConversationReferences
                .FirstOrDefaultAsync(cancellationToken: ct);

            if (globalChannelSetting is not null)
            {
                globalChannelSetting.ServiceUrl = convRef.ServiceUrl;
                globalChannelSetting.ConversationId = convRef.Conversation.Id;
            }
            else
            {
                var newGlobalSetting = new ConversationReference
                {
                    ServiceUrl = convRef.ServiceUrl,
                    ConversationId = convRef.Conversation.Id
                };

                _dbCtx.ConversationReferences.Add(newGlobalSetting);
            }

            await _dbCtx.SaveChangesAsync(ct);

            await context.SendActivityAsync(
                MessageFactory.Text($"✅ Set this channel for daily notifications!"), ct
            );
        }
        catch (Exception ex)
        {
            await context.SendActivityAsync(
                MessageFactory.Text($"❌ Error setting notification channel: {ex.Message}"), ct
            );
        }
    }

    public override string GetCommandName() => "Here";
    public override string GetCommandUsageExample() => "@adam here";
    public override string GetCommandDescription() => "Used to mark a channel as the one to send alert messages to.";
}