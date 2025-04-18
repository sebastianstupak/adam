using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public interface ICommand
{
    public Task HandleAsync(ITurnContext context, string[] cmdParts, CancellationToken ct);
}