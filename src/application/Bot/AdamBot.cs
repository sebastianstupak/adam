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
        await RouteCommandAsync(turnContext, parts, command, cancellationToken);
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

    private async Task RouteCommandAsync(ITurnContext<IMessageActivity> ctx, string[] parts, string command,
        CancellationToken ct)
    {
        switch (command.ToLowerInvariant())
        {
            case var c when StringMatchesStrings(c, CommandConstants.Subscribe, CommandConstants.Subscribe[..3],
                CommandConstants.Subscribe[..1]):
                await HandleSubscribeCommandAsync(ctx, parts, ct);
                break;

            case var c when StringMatchesStrings(c, CommandConstants.Unsubscribe, CommandConstants.Unsubscribe[..5],
                CommandConstants.Unsubscribe[..3]):
                await GetCommand<DeleteSubscriptionCommand>().HandleAsync(ctx, parts, ct);
                break;

            case var c when StringMatchesStrings(c, CommandConstants.Consent):
                await GetCommand<ConsentCommand>().HandleAsync(ctx, parts, ct);
                break;

            case var c when StringMatchesStrings(c, CommandConstants.Data):
                await GetCommand<DataCommand>().HandleAsync(ctx, parts, ct);
                break;

            case var c when StringMatchesStrings(c, CommandConstants.Here):
                await GetCommand<HereCommand>().HandleAsync(ctx, parts, ct);
                break;

            case var c when StringMatchesStrings(c, CommandConstants.Help):
                await HandleHelpCommandAsync(ctx, parts, ct);
                break;
        }
    }

    private async Task HandleSubscribeCommandAsync(ITurnContext<IMessageActivity> ctx, string[] parts,
        CancellationToken ct)
    {
        if (parts.Length < 3)
            return;

        switch (parts[2])
        {
            case CommandConstants.List:
                await GetCommand<ListSubscriptionsCommand>().HandleAsync(ctx, parts, ct);
                break;

            case CommandConstants.Company:
            case CommandConstants.Food:
                await GetCommand<CreateSubscriptionCommand>().HandleAsync(ctx, parts, ct);
                break;

            case CommandConstants.Update:
                await GetCommand<UpdateSubscriptionCommand>().HandleAsync(ctx, parts, ct);
                break;
        }
    }

    private async Task HandleHelpCommandAsync(ITurnContext<IMessageActivity> ctx, string[] parts, CancellationToken ct)
    {
        if (!HelpCommand.IsCommandsCacheInitialized())
            HelpCommand.InitCommandsCache(_commands);

        await GetCommand<HelpCommand>().HandleAsync(ctx, parts, ct);
    }

    #region Helpers

    private static bool StringMatchesStrings(string testee, params string[] targets)
    {
        return targets.Any(target => target.Equals(testee, StringComparison.InvariantCultureIgnoreCase));
    }

    private ICommand GetCommand<TCommand>() where TCommand : ICommand
    {
        return _commands.OfType<TCommand>().FirstOrDefault()
               ?? throw new InvalidOperationException(typeof(TCommand).Name + " not found.");
    }

    #endregion
}