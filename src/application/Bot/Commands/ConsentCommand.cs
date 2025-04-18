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
            await _userService.UpdateUserConsentAsync(context.Activity.From.Id);
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
}