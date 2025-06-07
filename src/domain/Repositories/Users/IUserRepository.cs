using ADAM.Domain.Models;

namespace ADAM.Domain.Repositories.Users;

public interface IUserRepository
{
    Task<User?> GetUserAsync(string teamsId);

    /// <summary>
    /// Returns all users whose even a single subscription matches at least a single string in <see cref="valuesToMatchAgainst"/>.
    /// </summary>
    /// <param name="valuesToMatchAgainst">A joined list of merchant names and offers</param>
    /// <returns>A collection of <see cref="User"/>s matching the predicate.</returns>
    Task<IEnumerable<(User user, IEnumerable<Subscription> subscriptions)>> GetUsersWithMatchingSubscriptionsAsync(
        IEnumerable<string> valuesToMatchAgainst);
    
    Task CreateUserAsync(string teamsId, string name);
}