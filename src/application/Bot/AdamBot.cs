using ADAM.Application.Bot.Commands;
using ADAM.Application.Services.Users;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace ADAM.Application.Bot;

public class AdamBot(IUserService userService, IEnumerable<ICommand> commands) : ActivityHandler
{
    private readonly IUserService _userService = userService;
    private readonly IEnumerable<ICommand> _commands = commands;

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var messageContent = turnContext.Activity.Text;
            var teamsId = turnContext.Activity.From.Id ??
                          throw new Exception("Unable to get user ID from interaction!");

            // Leave if it doesn't start with '@adam'
            if (!messageContent.StartsWith(CommandConstants.AdamBase, StringComparison.InvariantCultureIgnoreCase))
                return;

            // Leave if it doesn't have 2 or more params (e.g. is only '@adam') 
            var parts = messageContent.Trim().Split(" ");
            if (parts.Length < 2 ||
                !parts[0].Equals(CommandConstants.AdamBase, StringComparison.InvariantCultureIgnoreCase))
                return;

            var commandName = parts[1]; // e.g. help, here, subscribe, etc.

            // Check consent first (skip for consent/data commands)
            if (await UserRequiresConsentAsync(turnContext, cancellationToken, teamsId, commandName))
                return;

            // Required for @adam help to work
            if (!HelpCommand.IsCommandsCacheInitialized())
                HelpCommand.InitCommandsCache(_commands);

            // Route to appropriate command handler
            var cmd = RetrieveCommand(parts, commandName);
            if (cmd is null)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"❌ Unknown command"), cancellationToken);
                return;
            }

            await cmd.HandleAsync(turnContext, parts, cancellationToken);
        }
        catch (Exception ex)
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text($"❌ Unhandled error occurred: {ex.Message}"), cancellationToken);
        }
    }

    #region Helpers

    private ICommand? RetrieveCommand(string[] parts, string commandName)
    {
        // Get commands whose target command (e.g. help, subscribe, data, here, ...) matches
        var matchingCommands = _commands.Where(c =>
            c.GetCommandMatchTargets().CommandTargets.Contains(commandName, StringComparer.InvariantCultureIgnoreCase)
        ).ToList();

        if (!matchingCommands.Any())
            return null;

        // verify duplicate subcommands (for correct routing, two commands can't have the same subcommand(s))
        if (!AreSubcommandsUnique(matchingCommands, out var duplicates))
        {
            throw new Exception(
                $"Duplicate subcommands found for command '{commandName}': {string.Join(", ", duplicates)}"
            );
        }

        // retrieve the subcommand based on the command parts, or return the main command if no subcommand matches
        return matchingCommands.Count == 1 ? matchingCommands.First() : GetCommandOrSubcommand(parts, matchingCommands);
    }

    private static bool AreSubcommandsUnique(List<ICommand> matchingCommands, out IEnumerable<string> duplicates)
    {
        duplicates = matchingCommands
            .Where(c => c.GetCommandMatchTargets().SubcommandTargets != null)
            .SelectMany(c => c.GetCommandMatchTargets().SubcommandTargets!)
            .GroupBy(sub => sub, StringComparer.InvariantCultureIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        return !duplicates.Any();
    }

    private static ICommand? GetCommandOrSubcommand(string[] parts, List<ICommand> matchingCommands)
    {
        ICommand selectedCommand;

        // if parts > 2, it contains the subcommand name. [0] = @adam, [1] = command, [2] = subcommand, [3..] = params
        if (parts.Length > 2)
        {
            selectedCommand = matchingCommands.FirstOrDefault(c =>
                c.GetCommandMatchTargets().SubcommandTargets
                    ?.Contains(parts[2], StringComparer.InvariantCultureIgnoreCase) == true
            );
        }
        else
        {
            selectedCommand = matchingCommands.FirstOrDefault(c =>
                c.GetCommandMatchTargets().SubcommandTargets == null ||
                !c.GetCommandMatchTargets().SubcommandTargets.Any()
            );
        }

        return selectedCommand;
    }

    private static bool StringMatchesStrings(string testee, params string[] targets)
    {
        return targets.Any(target => target.Equals(testee, StringComparison.InvariantCultureIgnoreCase));
    }

    private async Task<bool> UserRequiresConsentAsync(ITurnContext<IMessageActivity> turnContext,
        CancellationToken cancellationToken,
        string teamsId, string command)
    {
        if (await _userService.DidUserAcceptDataStorageAsync(teamsId)
            || StringMatchesStrings(command, CommandConstants.Consent)
            || StringMatchesStrings(command, CommandConstants.Data))
            return false;

        await turnContext.SendActivityAsync(
            MessageFactory.Text("""
                                Hello, this might be our first interaction!

                                To use my services, please consent to your data being stored by typing `@adam consent`.
                                To learn what data we store about you, type `@adam data`.
                                """),
            cancellationToken
        );

        return true;
    }

    #endregion
}