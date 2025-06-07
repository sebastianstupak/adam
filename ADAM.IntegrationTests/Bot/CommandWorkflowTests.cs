using ADAM.Application.Bot;
using ADAM.Application.Bot.Commands;
using ADAM.Application.Services.Users;
using ADAM.Domain.Models;
using ADAM.IntegrationTests.TestConfiguration;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ConversationReference = ADAM.Domain.Models.ConversationReference;

namespace ADAM.IntegrationTests.Bot;

[Property("Infrastructure", "Container")]
[Property("Type", "Commands")]
public class CommandWorkflowTests(TestWebAppFactory factory) : IntegrationTestBase(factory)
{
    private IEnumerable<ICommand> _commands;
    private TestAdapter _adapter;
    private AdamBot _bot;
    private IUserService _userService;

    [Before(Test)]
    public override Task LoadDependenciesAsync()
    {
        _userService = factory.Services.GetRequiredService<IUserService>();
        _commands = factory.Services.GetRequiredService<IEnumerable<ICommand>>();
        _adapter = new TestAdapter();
        _bot = new AdamBot(_userService, DbCtx, _commands);

        return Task.CompletedTask;
    }

    protected override Task OnInitAsync()
    {
        DbCtx.ChangeTracker.Clear();
        return Task.CompletedTask;
    }

    protected override async Task OnDisposeAsync() => await DbCtx.Database.EnsureDeletedAsync();

    [Test]
    public async Task InitialBotInteraction_RespondsWithConsentInfo()
    {
        // Arrange, Act, Assert
        await new TestFlow(_adapter,
                async (turnContext, cancellationToken) => { await _bot.OnTurnAsync(turnContext, cancellationToken); })
            .Send("@adam")
            .AssertReplyContains("Hello, this might be our first interaction!")
            .Send("@adam")
            .AssertReplyContains("`@adam consent`")
            .StartTestAsync();
    }

    [Test, DependsOn(nameof(InitialBotInteraction_RespondsWithConsentInfo))]
    public async Task CreateUserSubscription_CreatesSubscription()
    {
        var dbCtx = DbCtx;

        // Arrange
        var teamsId = GetRandomString();
        var companySubscription = GetRandomString();
        var foodSubscription = GetRandomString();

        // Create a user with consent and conversation reference
        var user = new User
        {
            TeamsId = teamsId,
            Name = "FooBar",
            CreationDate = default,
            AcceptsDataStorage = true
        };

        await dbCtx.Users.AddAsync(user);
        await dbCtx.SaveChangesAsync();

        // Act & Assert
        await new TestFlow(_adapter, async (turnContext, cancellationToken) =>
            {
                // Set the From.Id on the Activity to match our test user
                turnContext.Activity.From = new ChannelAccount { Id = teamsId };

                await _bot.OnTurnAsync(turnContext, cancellationToken);
            })
            .Send($"@adam subscribe company {companySubscription}")
            .AssertReply("✅ Subscription created successfully.")
            .Send($"@adam subscribe food {foodSubscription}")
            .StartTestAsync();

        // Verify the subscription was created in the database
        var subscription = await dbCtx.Subscriptions
            .FirstOrDefaultAsync(s => s.User.Id == user.Id && s.Value == companySubscription);

        await Assert.That(subscription).IsNotNull();
        await Assert.That(subscription.Type).IsEqualTo(SubscriptionType.Merchant);
    }

    [Test, DependsOn(nameof(CreateUserSubscription_CreatesSubscription))]
    public async Task WhenUsingWithoutDataStorageConsent_CanNotUseBot()
    {
        var dbCtx = DbCtx;

        // Arrange
        var teamsId = GetRandomString();
        var companySubscription = GetRandomString();
        var foodSubscription = GetRandomString();

        // Create a user with consent and conversation reference
        var user = new User
        {
            TeamsId = teamsId,
            Name = "FooBar",
            CreationDate = default,
            AcceptsDataStorage = false
        };

        await dbCtx.Users.AddAsync(user);
        await dbCtx.SaveChangesAsync();

        // Act & Assert
        await new TestFlow(_adapter, async (turnContext, cancellationToken) =>
            {
                turnContext.Activity.From = new ChannelAccount { Id = teamsId };
                await _bot.OnTurnAsync(turnContext, cancellationToken);
            })
            .Send($"@adam subscribe company {companySubscription}")
            .AssertReplyContains("Hello, this might be our first interaction!")
            .Send($"@adam subscribe food {foodSubscription}")
            .AssertReplyContains("`@adam consent`")
            .Send("@adam consent")
            .AssertReplyContains("✅ Consent updated.")
            .Send($"@adam subscribe company {companySubscription}")
            .AssertReply("✅ Subscription created successfully.")
            .StartTestAsync();
    }

    [Test, DependsOn(nameof(WhenUsingWithoutDataStorageConsent_CanNotUseBot))]
    public async Task WhenUsingBot_PrefixWorksCorrectly()
    {
        // Act & Assert
        await new TestFlow(_adapter,
                async (turnContext, cancellationToken) => { await _bot.OnTurnAsync(turnContext, cancellationToken); })
            .Send("@adam")
            .AssertReplyContains("`@adam consent`")
            .Send("adam")
            .AssertNoReply()
            .Send("@ada")
            .AssertNoReply()
            .Send("@ad")
            .AssertNoReply()
            .Send("@a")
            .AssertNoReply()
            .Send("@adam")
            .AssertReplyContains("`@adam consent`")
            .StartTestAsync();
    }

    private ICommand GetCommand<TCommand>() where TCommand : ICommand
    {
        return _commands.OfType<TCommand>().FirstOrDefault()
               ?? throw new InvalidOperationException(typeof(TCommand).Name + " not found.");
    }

    private string GetRandomString() => Guid.NewGuid().ToString();
}