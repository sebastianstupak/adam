namespace ADAM.Domain.Repositories.Subscriptions;

public interface ISubscriptionRepository
{
    Task<List<Models.Subscription>> GetSubscriptionsAsync(Guid userGuid);
    Task<Models.Subscription?> GetSubscriptionAsync(long id);
    Task DeleteAsync(long id);
}