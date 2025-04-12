using ADAM.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Domain.Repositories.Users;

public class UserRepository(AppDbContext dbCtx) : IUserRepository
{
    public Task<Models.User?> GetUserAsync(Guid guid)
    {
        return dbCtx.Users.FirstOrDefaultAsync(u => u.Guid == guid);
    }

    // TODO: Write tests
    public async Task<IEnumerable<User>> GetUsersWithMatchingSubscriptionsAsync(IEnumerable<string> names)
    {
        {
            var foodNamesList = names.ToList();
            if (foodNamesList.Count == 0)
                return [];

            // Build a query that checks if any subscription (with wildcards) matches any food name
            var users = dbCtx.Users
                .Where(u => u.Subscriptions.Any(s =>
                    foodNamesList.Any(foodName =>
                        EF.Functions.ILike(foodName, "%" + s.Value + "%"))));
                

            var _string = users.ToQueryString();
            
            return await users.ToListAsync();
        }
    }

    public async Task CreateUserAsync(Guid guid)
    {
        dbCtx.Users.Add(new User
        {
            Guid = guid,
            CreationDate = DateTime.UtcNow
        });

        await dbCtx.SaveChangesAsync();
    }
}