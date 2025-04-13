using ADAM.Application.Objects;
using ADAM.Domain.Models;

namespace ADAM.Application.Services.Users;

public interface IUserService
{
    Task<IEnumerable<GetUserSubscriptionDto>> GetUserSubscriptionsAsync(string teamsId);

    Task CreateUserSubscriptionAsync(CreateUserSubscriptionDto dto);

    Task UpdateUserSubscriptionAsync(int id, UpdateUserSubscriptionDto dto, string teamsId);

    Task DeleteUserSubscriptionAsync(int id);
}