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
        var messageContent = turnContext.Activity.Text;
        var teamsId = turnContext.Activity.From.Id ?? throw new Exception("Unable to get user ID from interaction!");

        // Leave if it doesn't start with '@adam'
        if (!messageContent.StartsWith(CommandConstants.AdamBase, StringComparison.InvariantCultureIgnoreCase))
            return;

        // Leave if it doesn't have 2 or more params (e.g. is only '@adam') 
        var parts = messageContent.Trim().Split(" ");
        if (parts.Length < 2 ||
            !parts[0].Equals(CommandConstants.AdamBase, StringComparison.InvariantCultureIgnoreCase))
            return;

        var command = parts[1];

        // Check consent first (skip for consent/data commands)
        if (await UserRequiresConsentAsync(turnContext, cancellationToken, teamsId, command))
            return;

        // Route to appropriate command handler
        var cmd = RetrieveCommand(turnContext, parts, command, cancellationToken);
        await cmd.HandleAsync(turnContext, parts, cancellationToken);
    }

    #region Helpers

    private ICommand RetrieveCommand(ITurnContext<IMessageActivity> ctx, string[] parts, string commandName,
        CancellationToken ct)
    {
        // Get commands whose target command (e.g. help, subscribe, data, here, ...) matches
        var matchingCommands = _commands.Where(c =>
            c.GetCommandMatchTargets().Targets.Contains(commandName, StringComparer.InvariantCultureIgnoreCase)
        ).ToList();

        if (!matchingCommands.Any())
            throw new InvalidOperationException($"Command matching {commandName} not found.");

        // verify duplicate subcommands (for correct routing, two commands can't have the same subcommand(s))
        if (!AreSubcommandsUnique(matchingCommands, out var duplicateSubcommands))
        {
            throw new InvalidOperationException(
                $"Duplicate subcommands found for command '{commandName}': {string.Join(", ", duplicateSubcommands)}"
            );
        }

        // retrieve the subcommand based on the command parts, or return the main command if no subcommand matches
        return GetCommandOrSubcommand(parts, matchingCommands);
    }

    private static bool AreSubcommandsUnique(List<ICommand> matchingCommands, out IEnumerable<string> subcommands)
    {
        // get all subcommands
        var allSubcommands = matchingCommands
            .Where(c => c.GetCommandMatchTargets().SubcommandTargets != null)
            .SelectMany(c => c.GetCommandMatchTargets().SubcommandTargets!);

        // extract duplicates
        subcommands = allSubcommands
            .GroupBy(sub => sub, StringComparer.InvariantCultureIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        return !subcommands.Any();
    }

    private static ICommand GetCommandOrSubcommand(string[] parts, List<ICommand> matchingCommands)
    {
        ICommand selectedCommand;

        // if parts > 2, it contains the subcommand name. [0] = @adam, [1] = command, [2] = subcommand, [3..] = params
        if (parts.Length > 2)
        {
            var subcommandName = parts[2];

            // Select the command that matches the subcommand
            selectedCommand = matchingCommands.FirstOrDefault(c =>
                c.GetCommandMatchTargets().SubcommandTargets
                    ?.Contains(subcommandName, StringComparer.InvariantCultureIgnoreCase) == true
            );

            selectedCommand ??= matchingCommands.First();
        }
        else
        {
            // Select the first subcommands-less command if no subcommand is present, or fallback to the first previous one.
            selectedCommand = matchingCommands.FirstOrDefault(c =>
                c.GetCommandMatchTargets().SubcommandTargets == null ||
                !c.GetCommandMatchTargets().SubcommandTargets.Any()
            ) ?? matchingCommands.First();
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