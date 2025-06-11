using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public interface ICommand
{
    public Task HandleAsync(ITurnContext context, string[] cmdParts, CancellationToken ct);

    /// <summary>
    /// Returns a user-friendly name of the command. (e.g. if the command outputs a help section, this should return 'Help')
    /// </summary>
    public string GetCommandName();

    /// <summary>
    /// Returns an example of how a user might use this command.
    /// </summary>
    public string GetCommandUsageExample();

    /// <summary>
    /// Returns a brief description of what this command does. 
    /// </summary>
    public string GetCommandDescription();
}