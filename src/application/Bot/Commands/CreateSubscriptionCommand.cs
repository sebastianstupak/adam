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
            var subscriptionType = cmdParts[2].Equals(CommandConstants.Company,
                StringComparison.InvariantCultureIgnoreCase)
                ? SubscriptionType.Merchant
                : SubscriptionType.Offer;

            await _userService.CreateUserSubscriptionAsync(
                new CreateUserSubscriptionDto
                {
                    TeamsId = context.Activity.From.Id,
                    Type = subscriptionType,
                    Value = string.Join(" ", cmdParts[3..])
                }
            );

            await context.SendActivityAsync(
                MessageFactory.Text(
                    $"✅ Subscription for {(subscriptionType is SubscriptionType.Merchant ? "company" : "food")} created successfully."),
                ct
            );
        }
        catch (Exception e)
        {
            await context.SendActivityAsync(
                MessageFactory.Text($"❌ Error creating subscription: {e.Message}"), ct
            );
        }
    }

    public override string GetCommandName() => "Create Subscription";
    public override string GetCommandUsageExample() => "@adam s (food/company) (value)";
    public override string GetCommandDescription() => "Used to subscribe to a food or company alert during scalping.";

    public override CommandMatchTargets GetCommandMatchTargets() => new()
    {
        Targets = [CommandConstants.Subscribe, CommandConstants.Subscribe[..3], CommandConstants.Subscribe[..1]],
        SubcommandTargets = [CommandConstants.Food, CommandConstants.Company]
    };
}