namespace ADAM.Domain.Repositories.Users;

public interface IUserRepository
{
    Task<Models.User?> GetUserAsync (Guid guid);
}