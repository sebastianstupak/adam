using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public abstract class Command : ICommand
{
    public Task HandleAsync(ITurnContext context, string[] cmdParts, CancellationToken ct)
    {
        try
        {
            return HandleCommandAsync(context, cmdParts, ct);
        }
        catch (Exception ex)
        {
            return context.SendActivityAsync(
                MessageFactory.Text($"❌ Unhandled error executing a command. {ex}"), ct
            );
        }
    }

    protected abstract Task HandleCommandAsync(
        ITurnContext context,
        string[] cmdParts,
        CancellationToken ct
    );
}