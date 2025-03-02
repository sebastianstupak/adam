using Microsoft.EntityFrameworkCore;

namespace ADAM.Domain.Repositories.Users;

public class UserRepository(AppDbContext dbCtx) : IUserRepository
{
    public Task<Models.User?> GetUserAsync(Guid guid)
    {
        return dbCtx.Users.FirstOrDefaultAsync(u => u.Guid == guid);
    }
}