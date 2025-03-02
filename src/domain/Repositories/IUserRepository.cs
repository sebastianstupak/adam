using ADAM.Domain.Models;

namespace ADAM.Domain.Repositories;

public interface IUserRepository
{
    Task<List<Subscription>> GetUserSubscriptionsAsync(Guid userGuid);
}