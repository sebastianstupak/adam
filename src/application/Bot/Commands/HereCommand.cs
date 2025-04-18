using ADAM.Domain;
using Microsoft.Bot.Builder;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Application.Bot.Commands;

public class HereCommand(AppDbContext dbCtx) : Command
{
    private readonly AppDbContext _dbCtx;

    protected override async Task HandleCommandAsync(ITurnContext context, string[] cmdParts, CancellationToken ct)
    {
        try
        {
            var convRef = context.Activity.GetConversationReference();
            ArgumentNullException.ThrowIfNull(convRef);

            var user = await _dbCtx.Users.FirstOrDefaultAsync(u =>
                u.TeamsId == context.Activity.From.Id, cancellationToken: ct);

            ArgumentNullException.ThrowIfNull(user);

            var crExists =
                await _dbCtx.ConversationReferences.AnyAsync(cr => cr.UserId == user.Id, cancellationToken: ct);

            if (crExists)
            {
                var crToUpdate = await _dbCtx.ConversationReferences
                    .Where(cr => cr.UserId == user.Id)
                    .FirstAsync(cancellationToken: ct);

                crToUpdate.ServiceUrl = convRef.ServiceUrl;
                crToUpdate.ConversationId = convRef.Conversation.Id;
            }
            else
            {
                var obj = new Domain.Models.ConversationReference
                {
                    UserId = user.Id,
                    ServiceUrl = convRef.ServiceUrl,
                    ConversationId = convRef.Conversation.Id
                };
                _dbCtx.ConversationReferences.Add(obj);
            }

            await _dbCtx.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            await context.SendActivityAsync(
                MessageFactory.Text($"❌ Error setting current channel for contact: {ex.Message}"), ct
            );
        }
    }
}