using ADAM.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Domain.Repositories.Users;

public class UserRepository(AppDbContext dbCtx) : IUserRepository
{
    public Task<Models.User?> GetUserAsync(Guid guid)
    {
        return dbCtx.Users.FirstOrDefaultAsync(u => u.Guid == guid);
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