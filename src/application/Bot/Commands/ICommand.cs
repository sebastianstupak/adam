using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public interface ICommand
{
    public Task HandleAsync(ITurnContext turnContext, string[] commandParts, CancellationToken cancellationToken);
}