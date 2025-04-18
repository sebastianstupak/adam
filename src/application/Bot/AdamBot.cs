using ADAM.Application.Bot.Commands;
using ADAM.Application.Services.Users;
using ADAM.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Application.Bot;

public class AdamBot(
    IUserService userService,
    AppDbContext dbCtx,
    IEnumerable<ICommand> commands
) : ActivityHandler
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

        if (!StringMatchesFullOrSubString(parts.ElementAtOrDefault(1) ?? "", CommandConstants.Consent))
            await PersistConversationReferenceAsync(teamsId, turnContext);

        if (StringMatchesFullOrSubString(parts[1], CommandConstants.Subscribe, [0, 3, 0, 1]))
        {
            switch (parts[2])
            {
                case CommandConstants.List:
                {
                    await GetCommand<ListSubscriptionsCommands>().HandleAsync(turnContext, parts, cancellationToken);
                    break;
                }

                case CommandConstants.Company:
                case CommandConstants.Food:
                {
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
    }

    #region Helpers

    private static bool StringMatchesFullOrSubString(string testee, string target, int[]? substringLimits = null)
    {
        var match = false;

        if (substringLimits is null)
            return testee.Equals(target, StringComparison.InvariantCultureIgnoreCase);

        if (substringLimits.Length % 2 != 0)
            throw new ArgumentException($"{nameof(substringLimits)} must have an even count of values.");

        var substringLimitPairs = new List<(int, int)>();
        for (var i = 0; i < substringLimits.Length; i += 2)
        {
            if (i + 1 < substringLimits.Length)
                substringLimitPairs.Add((substringLimits[i], substringLimits[i + 1]));
        }

        foreach (var pair in substringLimitPairs)
        {
            if (testee.Equals(target, StringComparison.InvariantCultureIgnoreCase)
                || testee.Equals(target.Substring(pair.Item1, pair.Item2), StringComparison.InvariantCultureIgnoreCase))
            {
                match = true;
                break;
            }
        }

        return match;
    }

    private ICommand GetCommand<TCommand>() where TCommand : ICommand
    {
        return _commands.OfType<TCommand>().FirstOrDefault()
               ?? throw new InvalidOperationException(typeof(TCommand).Name + " not found.");
    }

    #endregion

    private async Task PersistConversationReferenceAsync(string teamsId, ITurnContext turnContext)
    {
        var convRef = turnContext.Activity.GetConversationReference();
        ArgumentNullException.ThrowIfNull(convRef);

        var user = await _dbCtx.Users.FirstOrDefaultAsync(u =>
            u.TeamsId == teamsId
        );
        ArgumentNullException.ThrowIfNull(user);

        var crExists = await _dbCtx.ConversationReferences.AnyAsync(cr => cr.UserId == user.Id);

        if (crExists)
        {
            var crToUpdate = await _dbCtx.ConversationReferences
                .Where(cr => cr.UserId == user.Id)
                .FirstAsync();

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

        await _dbCtx.SaveChangesAsync();
    }
}