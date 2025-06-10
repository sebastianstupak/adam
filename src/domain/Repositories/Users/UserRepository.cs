using ADAM.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Domain.Repositories.Users;

public class UserRepository(AppDbContext dbCtx) : IUserRepository
{
    private readonly AppDbContext _dbCtx = dbCtx;

    public Task<User?> GetUserAsync(string teamsId)
    {
        return _dbCtx.Users
            .Include(u => u.Subscriptions)
            .FirstOrDefaultAsync(u => u.TeamsId == teamsId);
    }

    public async Task<IEnumerable<(User user, IEnumerable<Subscription> subscriptions)>>
        GetUsersWithMatchingSubscriptionsAsync(IEnumerable<string> valuesToMatchAgainst)
    {
        var foodNamesList = valuesToMatchAgainst.ToList();

        if (foodNamesList.Count == 0)
            return [];

        var query = _dbCtx.Users
            .Where(u => u.Subscriptions.Any(s =>
                foodNamesList.Any(foodName => EF.Functions.Like(foodName, "%" + s.Value + "%")
                )
            ))
            .Select(u => new
            {
                User = u,
                // Select matching subscription values
                MatchingSubscriptions = u.Subscriptions
                    .Where(s => foodNamesList.Any(foodName => EF.Functions.Like(foodName, "%" + s.Value + "%")
                    ))
            });

        var results = await query.ToListAsync();
        return results.Select(x => (x.User, x.MatchingSubscriptions));
    }

    public async Task CreateUserAsync(string teamsId, string name)
    {
        _dbCtx.Users.Add(
            new User
            {
                TeamsId = teamsId,
                Name = name,
                CreationDate = DateTime.UtcNow,
                AcceptsDataStorage = false
            }
        );

        await _dbCtx.SaveChangesAsync();
    }
}