using ADAM.Application.Objects;
using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

[Command]
public class DataCommand : Command
{
    protected override async Task HandleCommandAsync(ITurnContext context, string[] cmdParts, CancellationToken ct)
    {
        await context.SendActivityAsync(
            MessageFactory.Text("""
                                # User Data Storage

                                We store the following data regarding you, the end user:
                                - Teams ID
                                - Teams Name
                                - The date of your user entry creation (typically via `@adam consent`)
                                - Whether you've consented to data storage (typically via `@adam consent`)
                                - Your ADAM subscriptions

                                If you have any more questions, please contact the devs of ADAM.
                                """),
            ct
        );
    }

    public override string GetCommandName() => "Data";
    public override string GetCommandUsageExample() => "@adam data";
    public override string GetCommandDescription() => "Shows what data we store about you.";

    public override CommandMatchTargets GetCommandMatchTargets() => new()
    {
        CommandTargets = [CommandConstants.Data],
        SubcommandTargets = null
    };
}