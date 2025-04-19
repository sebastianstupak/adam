using ADAM.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Domain.Repositories.Users;

public class UserRepository(AppDbContext dbCtx) : IUserRepository
{
    private readonly AppDbContext _dbCtx = dbCtx;
    
    public Task<User?> GetUserAsync(string teamsId)
    {
        return _dbCtx.Users.FirstOrDefaultAsync(u => u.TeamsId == teamsId);
    }

    // TODO: Write tests
    // public async Task<IEnumerable<(User user, IEnumerable<string> subscriptions)>>
    //     GetUsersWithMatchingSubscriptionsAsync(IEnumerable<string> names)
    // {
    //     {
    //         var foodNamesList = names.ToList();
    //
    //         if (foodNamesList.Count == 0)
    //             return [];
    //
    //         return await dbCtx.Users
    //             .Where(u => u.Subscriptions.Any(
    //                 s => foodNamesList.Any(
    //                     foodName => EF.Functions.ILike(foodName, "%" + s.Value + "%")
    //                 )
    //             ))
    //             .ToListAsync();
    //     }
    // }

    public async Task<IEnumerable<(User user, IEnumerable<Subscription> subscriptions)>>
        GetUsersWithMatchingSubscriptionsAsync(IEnumerable<string> names)
    {
        var foodNamesList = names.ToList();
    
        if (foodNamesList.Count == 0)
            return [];
    
        var query = _dbCtx.Users
            .Where(u => u.Subscriptions.Any(
                s => foodNamesList.Any(
                    foodName => EF.Functions.ILike(foodName, "%" + s.Value + "%")
                )
            ))
            .Select(u => new
            {
                User = u,
                // Select matching subscription values
                MatchingSubscriptions = u.Subscriptions
                    .Where(s => foodNamesList.Any(
                        foodName => EF.Functions.ILike(foodName, "%" + s.Value + "%")
                    ))
            });
    
        var results = await query.ToListAsync();
        return results.Select(x => (x.User, x.MatchingSubscriptions));
    }

    public async Task CreateUserAsync(string teamsId)
    {
        _dbCtx.Users.Add(new User
        {
            TeamsId = teamsId,
            CreationDate = DateTime.UtcNow,
            AcceptsDataStorage = false
        });

        await _dbCtx.SaveChangesAsync();
    }
}