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

            var user = await _dbCtx.Users.FirstOrDefaultAsync(u =>
                u.TeamsId == context.Activity.From.Id, cancellationToken: ct);

            ArgumentNullException.ThrowIfNull(user);

            var convRefToUpdate = await _dbCtx.ConversationReferences
                .Where(cr => cr.UserId == user.Id)
                .FirstOrDefaultAsync(cancellationToken: ct);

            if (convRefToUpdate is not null)
            {
                convRefToUpdate.ServiceUrl = convRef.ServiceUrl;
                convRefToUpdate.ConversationId = convRef.Conversation.Id;
            }
            else
            {
                var newConvRef = new ConversationReference
                {
                    UserId = user.Id,
                    ServiceUrl = convRef.ServiceUrl,
                    ConversationId = convRef.Conversation.Id
                };

                _dbCtx.ConversationReferences.Add(newConvRef);
            }

            await _dbCtx.SaveChangesAsync(ct);

            await context.SendActivityAsync(
                MessageFactory.Text($"✅ I will message you in this channel from now on!"), ct
            );
        }
        catch (Exception ex)
        {
            await context.SendActivityAsync(
                MessageFactory.Text($"❌ Error setting current channel for contact: {ex.Message}"), ct
            );
        }
    }
}