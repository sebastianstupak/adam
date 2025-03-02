namespace ADAM.Domain.Repositories.Subscriptions;

public interface ISubscriptionRepository
{
    Task<List<Models.Subscription>> GetSubscriptionsAsync(Guid userGuid);
    Task<int> CreateSubscriptionAsync(Models.Subscription subscription);
    Task<Models.Subscription?> GetSubscriptionAsync(long id);
    Task DeleteAsync(long id);
}