using ADAM.Application.Objects;
using ADAM.Domain.Models;

namespace ADAM.Application.Services.Users;

public interface IUserService
{
    Task<IEnumerable<GetUserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userGuid);

    Task CreateUserSubscriptionAsync(CreateUserSubscriptionDto dto);

    Task UpdateUserSubscriptionAsync(int id, UpdateUserSubscriptionDto dto);

    Task DeleteUserSubscriptionAsync(int id);
}