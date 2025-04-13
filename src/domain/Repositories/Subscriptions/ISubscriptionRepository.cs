using ADAM.Domain.Models;

namespace ADAM.Domain.Repositories.Subscriptions;

public interface ISubscriptionRepository
{
    Task<List<Subscription>> GetSubscriptionsAsync(string teamsId);
    Task<Models.Subscription?> GetSubscriptionAsync(long id);
    Task DeleteAsync(long id);
}