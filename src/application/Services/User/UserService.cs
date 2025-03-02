using ADAM.Domain.Models;
using ADAM.Domain.Repositories;

namespace ADAM.Application.Services.User;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<List<Subscription>> GetUserSubscriptionsAsync(Guid userGuid)
    {
        return await userRepository.GetUserSubscriptionsAsync(userGuid);
    }
}