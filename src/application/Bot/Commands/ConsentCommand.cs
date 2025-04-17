using ADAM.Application.Services.Users;
using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public class ConsentCommand(IUserService userService) : ICommand
{
    private readonly IUserService _userService = userService;

    public async Task HandleAsync(ITurnContext turnContext, string[] commandParts, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.UpdateUserConsentAsync(turnContext.Activity.From.Id);
            await turnContext.SendActivityAsync(
                MessageFactory.Text("✅ Consent updated."), cancellationToken
            );
        }
        catch
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("❌ Error updating consent!"), cancellationToken
            );
        }
    }
}