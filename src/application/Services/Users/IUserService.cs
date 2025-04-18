using System.Collections;
using ADAM.Application.Objects;
using ADAM.Domain.Models;

namespace ADAM.Application.Services.Users;

public interface IUserService
{
    Task CreateUserAsync(string teamsId);
    
    Task<IEnumerable<GetUserSubscriptionDto>> GetUserSubscriptionsAsync(string teamsId);

    Task CreateUserSubscriptionAsync(CreateUserSubscriptionDto dto);

    Task UpdateUserSubscriptionAsync(int id, UpdateUserSubscriptionDto dto, string teamsId);

    Task DeleteUserSubscriptionAsync(int id);
    
    Task<bool> DidUserAcceptDataStorageAsync(string teamsId);
    Task UpdateUserConsentAsync(string teamsId);
    Task<IEnumerable<(User u, string message)>> GetUsersWithMatchingSubscriptionsAsync(
        List<string> merchantNamesAndMeals);
}