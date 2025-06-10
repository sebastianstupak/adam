using ADAM.Application.Objects;
using ADAM.Application.Services.Users;
using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public class ConsentCommand(IUserService userService) : Command
{
    private readonly IUserService _userService = userService;

    protected override async Task HandleCommandAsync(ITurnContext context, string[] cmdParts, CancellationToken ct)
    {
        try
        {
            await _userService.UpdateUserConsentAsync(context.Activity.From.Id, context.Activity.From.Name);
            await context.SendActivityAsync(
                MessageFactory.Text("""
                                    ✅ Consent updated.

                                    To receive alerts based on your subscriptions, set a channel I should message you in using `@adam here`.
                                    """), ct
            );
        }
        catch
        {
            await context.SendActivityAsync(
                MessageFactory.Text("❌ Error updating consent!"), ct
            );
        }
    }

    public override string GetCommandName() => "Consent";
    public override string GetCommandUsageExample() => "@adam consent";
    public override string GetCommandDescription() => "Used to consent to having your data saved.";

    public override CommandMatchTargets GetCommandMatchTargets() => new()
    {
        CommandTargets = [CommandConstants.Consent],
        SubcommandTargets = null
    };
}