using ADAM.Domain.Models;
using ADAM.Domain.Repositories.Users;
using ADAM.IntegrationTests.TestConfiguration;
using Microsoft.EntityFrameworkCore;

namespace ADAM.IntegrationTests.Domain;

[ClassDataSource<TestWebAppFactory>(Shared = SharedType.Keyed, Key = "Subscriptions")]
[Property("TestCategory", "Container")]
public class UserRepositoryTests(TestWebAppFactory factory) : IntegrationTestBase(factory)
{
    private readonly List<string> _names = ["bryndzové halušky", "KFC", "naan & curry", "tahiti"];

    private UserRepository _userRepository;

    protected override Task OnInitAsync()
    {
        DbCtx.ChangeTracker.Clear();
        return Task.CompletedTask;
    }

    [Before(Test)]
    public override Task LoadDependenciesAsync()
    {
        _userRepository = new UserRepository(DbCtx);
        return Task.CompletedTask;
    }

    protected override async Task OnDisposeAsync() => await DbCtx.Database.EnsureDeletedAsync();

    [Test]
    [Arguments("bryndz", SubscriptionType.Offer, true)]
    [Arguments("bryndza", SubscriptionType.Offer, false)]
    [Arguments("kfcc", SubscriptionType.Merchant, false)]
    [Arguments("kfc", SubscriptionType.Merchant, true)]
    [Arguments("naan", SubscriptionType.Merchant, true)]
    [Arguments("tahiti", SubscriptionType.Merchant, true)]
    [Arguments("tahit", SubscriptionType.Offer, true)]
    [Arguments("b", SubscriptionType.Offer, true)]
    [Arguments("bryndzove", SubscriptionType.Offer, false)]
    [NotInParallel(nameof(GetUsersWithMatchingSubscriptionsAsync_WhenASubscriptionMatchesPassedName_ReturnsUser))]
    public async Task GetUsersWithMatchingSubscriptionsAsync_WhenASubscriptionMatchesPassedName_ReturnsUser(
        string subscriptionValue, SubscriptionType subscriptionType, bool shouldReturnUser)
    {
        DbCtx.Users.Add(new User
        {
            Guid = Guid.NewGuid(),
            CreationDate = DateTime.UtcNow,
            Subscriptions =
            [
                new Subscription
                {
                    Type = subscriptionType,
                    Value = subscriptionValue,
                }
            ]
        });

        await DbCtx.SaveChangesAsync();

        DbCtx.ChangeTracker.Clear();

        var users = (await _userRepository.GetUsersWithMatchingSubscriptionsAsync(_names)).ToList();

        try
        {
            await Assert.That(
                (users.Count != 0 && shouldReturnUser) || (users.Count == 0 && !shouldReturnUser)
            ).IsTrue();
        }
        finally
        {
            Console.WriteLine($"{users.Count} - {shouldReturnUser}\n{subscriptionValue} {subscriptionType}");
            await DbCtx.Users.ExecuteDeleteAsync();
        }
    }
}