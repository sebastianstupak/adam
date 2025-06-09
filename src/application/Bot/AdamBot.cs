using ADAM.Application.Bot.Commands;
using ADAM.Application.Services.Users;
using ADAM.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Application.Bot;

public class AdamBot(IUserService userService, AppDbContext dbCtx, IEnumerable<ICommand> commands) : ActivityHandler
{
    private readonly IUserService _userService = userService;
    private readonly AppDbContext _dbCtx = dbCtx;
    private readonly IEnumerable<ICommand> _commands = commands;

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
        CancellationToken cancellationToken)
    {
        var messageContent = turnContext.Activity.Text;

        var teamsId = turnContext.Activity.From.Id
                      ?? throw new Exception("Unable to get user ID from interaction!");

        if (!messageContent.StartsWith(CommandConstants.AdamBase, StringComparison.InvariantCultureIgnoreCase))
            return;

        var parts = messageContent.Split(" ");
        if (!parts[0].Equals(CommandConstants.AdamBase, StringComparison.InvariantCultureIgnoreCase))
            return;

        if (!await _userService.DidUserAcceptDataStorageAsync(teamsId)
            && !StringMatchesFullOrSubString(parts.ElementAtOrDefault(1) ?? "", CommandConstants.Consent))
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("""
                                    Hello, this might be our first interaction!

                                    To use my services, please consent to your data being stored by typing `@adam consent`.
                                    To learn what data we store about you, type `@adam data`.
                                    """),
                cancellationToken
            );

            return;
        }

        if (parts.Length < 2)
            return;

        if (StringMatchesFullOrSubString(parts[1], CommandConstants.Subscribe, [0, 3, 0, 1]))
        {
            switch (parts[2])
            {
                case CommandConstants.List:
                {
                    await GetCommand<ListSubscriptionsCommand>().HandleAsync(turnContext, parts, cancellationToken);
                    break;
                }

                case CommandConstants.Company:
                case CommandConstants.Food:
                {
                    var user = await _dbCtx.Users.FirstOrDefaultAsync(
                        u => u.TeamsId == teamsId,
                        cancellationToken: cancellationToken
                    );

                    if (user is null)
                    {
                        await turnContext.SendActivityAsync(
                            MessageFactory.Text("""
                                                ❌ No channel set!

                                                Let me know where to alert you using `@adam here`.
                                                """),
                            cancellationToken
                        );

                        return;
                    }

                    await GetCommand<CreateSubscriptionCommand>().HandleAsync(turnContext, parts, cancellationToken);
                    break;
                }

                case CommandConstants.Update:
                {
                    await GetCommand<UpdateSubscriptionCommand>().HandleAsync(turnContext, parts, cancellationToken);
                    break;
                }
            }
        }

        if (StringMatchesFullOrSubString(parts[1], CommandConstants.Unsubscribe, [0, 5, 0, 3]))
        {
            await GetCommand<DeleteSubscriptionCommand>().HandleAsync(turnContext, parts, cancellationToken);
        }

        if (StringMatchesFullOrSubString(parts[1], CommandConstants.Consent))
        {
            await GetCommand<ConsentCommand>().HandleAsync(turnContext, parts, cancellationToken);
        }

        if (StringMatchesFullOrSubString(parts[1], CommandConstants.Here))
        {
            await GetCommand<HereCommand>().HandleAsync(turnContext, parts, cancellationToken);
        }

        if (StringMatchesFullOrSubString(parts[1], CommandConstants.Help))
        {
            if (!HelpCommand.IsCommandsCacheInitialized())
                HelpCommand.InitCommandsCache(_commands);

            await GetCommand<HelpCommand>().HandleAsync(turnContext, parts, cancellationToken);
        }
    }

    #region Helpers

    /// <summary>
    /// Checks whether a string matches a target string, or any of its substrings declared as index pairs.
    /// </summary>
    /// <param name="testee">A string to test</param>
    /// <param name="target">The target to match against</param>
    /// <param name="substringLimits">Optional array of integers,
    ///              where every two entries are considered the beginning and end indexes of the target.</param>
    /// <returns>TRUE, if matches</returns>
    /// <exception cref="ArgumentException">In case the int[] of pairs contains an odd amount of values.</exception>
    private static bool StringMatchesFullOrSubString(string testee, string target, int[]? substringLimits = null)
    {
        var match = false;

        if (substringLimits is null)
            return testee.Equals(target, StringComparison.InvariantCultureIgnoreCase);

        if (substringLimits.Length % 2 != 0)
            throw new ArgumentException($"{nameof(substringLimits)} must have an even count of values.");

        var substringLimitPairs = new List<(int, int)>();
        for (var i = 0; i < substringLimits.Length; i += 2) // Create pairs
        {
            if (i + 1 < substringLimits.Length)
                substringLimitPairs.Add((substringLimits[i], substringLimits[i + 1]));
        }

        var matchesFully = testee.Equals(target, StringComparison.InvariantCultureIgnoreCase);

        foreach (var pair in substringLimitPairs)
        {
            var matchesSubstringPair = testee.Equals(target.Substring(pair.Item1, pair.Item2),
                StringComparison.InvariantCultureIgnoreCase);

            if (!matchesFully && !matchesSubstringPair)
                continue;

            match = true;
            break;
        }

        return match;
    }

    private ICommand GetCommand<TCommand>() where TCommand : ICommand
    {
        return _commands.OfType<TCommand>().FirstOrDefault()
               ?? throw new InvalidOperationException(typeof(TCommand).Name + " not found.");
    }

    #endregion
}