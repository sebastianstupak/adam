using ADAM.Domain.Models;
using ADAM.Domain.Repositories.Users;
using ADAM.IntegrationTests.TestConfiguration;

namespace ADAM.IntegrationTests.Domain;

[ClassDataSource<TestWebAppFactory>(Shared = SharedType.Keyed, Key = "Subscriptions")]
[Property("TestCategory", "Container")]
public class UserRepositoryTests : IntegrationTestBase
{
    private readonly List<string> _names = ["bryndzové halušky", "KFC", "naan & curry"];

    private UserRepository _userRepository;

    public UserRepositoryTests(TestWebAppFactory factory) : base(factory)
    {
    }

    protected override Task OnInitAsync()
    {
        DbCtx.ChangeTracker.Clear();
        return Task.CompletedTask;
    }

    protected override async Task OnDisposeAsync() => await DbCtx.Database.EnsureDeletedAsync();

    [Test]
    [Arguments("bryndz", SubscriptionType.Offer, false)]
    [Arguments("bryndza", SubscriptionType.Offer, true)]
    [Arguments("kfcc", SubscriptionType.Merchant, true)]
    [Arguments("kfc", SubscriptionType.Merchant, false)]
    [Arguments("naan", SubscriptionType.Merchant, false)]
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

        await Assert.That((users.Count != 0 && shouldReturnUser) || (users.Count == 0 && !shouldReturnUser)).IsTrue();
    }

    [Before(Test)]
    public override Task LoadDependenciesAsync()
    {
        _userRepository = new UserRepository(DbCtx);
        return Task.CompletedTask;
    }
}