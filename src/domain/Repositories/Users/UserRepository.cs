using ADAM.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Domain.Repositories.Users;

public class UserRepository(AppDbContext dbCtx) : IUserRepository
{
    public Task<Models.User?> GetUserAsync(string teamsId)
    {
        return dbCtx.Users.FirstOrDefaultAsync(u => u.TeamsId == teamsId);
    }

    // TODO: Write tests
    public async Task<IEnumerable<User>> GetUsersWithMatchingSubscriptionsAsync(IEnumerable<string> names)
    {
        {
            var foodNamesList = names.ToList();

            if (foodNamesList.Count == 0)
                return [];

            return await dbCtx.Users
                .Where(u => u.Subscriptions.Any(
                        s => foodNamesList.Any(
                            foodName => EF.Functions.ILike(foodName, "%" + s.Value + "%")
                        )
                    )
                )
                .ToListAsync();
        }
    }

    public async Task CreateUserAsync(string teamsId)
    {
        dbCtx.Users.Add(new User
        {
            TeamsId = teamsId,
            CreationDate = DateTime.UtcNow,
            AcceptsDataStorage = false
        });

        await dbCtx.SaveChangesAsync();
    }
}