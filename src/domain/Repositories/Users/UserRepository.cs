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
        IQueryable<User> query = null;
    
        foreach (var name in names)
        {
            var pattern = "%" + name + "%";
        
            var nameQuery = dbCtx.Users
                .Where(u => u.Subscriptions.Any(s => EF.Functions.ILike(s.Value, pattern)));
        
            query = query == null ? nameQuery : query.Union(nameQuery);
        }
    
        if (query == null)
            return Enumerable.Empty<User>();
        
        return await query.ToListAsync();
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