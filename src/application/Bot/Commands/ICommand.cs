using ADAM.Application.Objects;
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

    /// <summary>
    /// Returns an object containing two collections of strings to match user input against.
    /// </summary>
    /// <example>
    ///     If this is the help command, the return object has a single string of "help" in the Targets collection.<br/>
    ///     If this is the subscribe create command, the return object has "subscribe," "sub," and "s" in the Targets collection, and "food" and "company" in the subcommands collection.
    /// </example>
    public CommandMatchTargets GetCommandMatchTargets();
}