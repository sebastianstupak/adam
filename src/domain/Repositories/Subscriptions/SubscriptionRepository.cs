using ADAM.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Domain.Repositories.Subscriptions;

public class SubscriptionRepository(AppDbContext dbCtx) : ISubscriptionRepository
{
    public async Task<Subscription?> GetSubscriptionAsync(long subscriptionId)
    {
        return await dbCtx.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);
    }

    public async Task<int> DeleteAsync(long id)
    {
        return await dbCtx.Subscriptions
            .Where(s => s.Id == id)
            .ExecuteDeleteAsync();
    }

    public async Task<List<Subscription>> GetSubscriptionsAsync(Guid userGuid)
    {
        return await dbCtx.Subscriptions
            .Where(s => s.User.Guid == userGuid)
            .ToListAsync();
    }
}