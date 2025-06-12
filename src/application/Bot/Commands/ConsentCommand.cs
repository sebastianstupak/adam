using ADAM.Application.Objects;
using ADAM.Application.Services.Users;
using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

[Command("Consent", "@adam consent", "Used to consent to having your data saved.")]
public class ConsentCommand(IUserService userService) : Command
{
    private readonly IUserService _userService = userService;

    protected override async Task HandleCommandAsync(ITurnContext context, string[] cmdParts, CancellationToken ct)
    {
        try
        {
            await _userService.UpdateUserConsentAsync(context.Activity.From.Id, context.Activity.From.Name);
            await context.SendActivityAsync(
                MessageFactory.Text("✅ Consent updated."), ct
            );
        }
        catch (Exception ex)
        {
            await context.SendActivityAsync(
                MessageFactory.Text($"❌ Error updating consent: {ex.Message}"), ct
            );
        }
    }

    public override CommandMatchTargets GetCommandMatchTargets() => new()
    {
        CommandTargets = [CommandConstants.Consent],
        SubcommandTargets = null
    };
}