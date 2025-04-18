using ADAM.Application.Objects;
using ADAM.Application.Services.Users;
using ADAM.Domain.Models;
using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public class CreateSubscriptionCommand(IUserService userService) : Command
{
    private readonly IUserService _userService = userService;

    protected override async Task HandleCommandAsync(ITurnContext context, string[] cmdParts, CancellationToken ct)
    {
        try
        {
            await _userService.CreateUserSubscriptionAsync(new CreateUserSubscriptionDto
            {
                TeamsId = context.Activity.From.Id,
                Type = cmdParts[2].Equals(CommandConstants.Company,
                    StringComparison.InvariantCultureIgnoreCase)
                    ? SubscriptionType.Merchant
                    : SubscriptionType.Offer,
                Value = string.Join(" ", cmdParts[3..])
            });

            await context.SendActivityAsync(
                MessageFactory.Text("✅ Subscription created successfully."), ct
            );
        }
        catch (Exception e)
        {
            await context.SendActivityAsync(
                MessageFactory.Text($"❌ Error creating subscription: {e.Message}"), ct
            );
        }
    }
}