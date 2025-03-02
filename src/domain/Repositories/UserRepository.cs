using ADAM.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Domain.Repositories;

public class UserRepository(AppDbContext dbCtx) : IUserRepository
{
    public Task<List<Subscription>> GetUserSubscriptionsAsync(Guid userGuid)
    {
        return dbCtx.Subscriptions
            .Where(s => s.User.Guid == userGuid)
            .ToListAsync();
    }
}