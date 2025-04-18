using ADAM.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Domain.Repositories.Subscriptions;

public class SubscriptionRepository(AppDbContext dbCtx) : ISubscriptionRepository
{
    public async Task<Subscription?> GetSubscriptionAsync(long subscriptionId)
    {
        return await dbCtx.Subscriptions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);
    }

    public async Task DeleteAsync(long id)
    {
        var amount = await dbCtx.Subscriptions
            .Where(s => s.Id == id)
            .ExecuteDeleteAsync();
    }

    public async Task<List<Subscription>> GetSubscriptionsAsync(string teamsId)
    {
        return await dbCtx.Subscriptions
            .Where(s => s.User.TeamsId == teamsId)
            .ToListAsync();
    }
}