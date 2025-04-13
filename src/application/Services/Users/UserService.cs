using ADAM.Application.Objects;
using ADAM.Domain;
using ADAM.Domain.Models;
using ADAM.Domain.Repositories.Subscriptions;
using ADAM.Domain.Repositories.Users;

namespace ADAM.Application.Services.Users;

public class UserService(
    ISubscriptionRepository subscriptionRepository,
    IUserRepository userRepository,
    AppDbContext dbCtx
) : IUserService
{
    public async Task<IEnumerable<GetUserSubscriptionDto>> GetUserSubscriptionsAsync(string teamsId)
    {
        var user = await userRepository.GetUserAsync(teamsId);

        if (user is null)
            await userRepository.CreateUserAsync(teamsId);

        var subscriptions = await subscriptionRepository.GetSubscriptionsAsync(teamsId);

        return subscriptions.Select(s => new GetUserSubscriptionDto
        {
            Id = s.Id,
            Type = s.Type,
            Value = s.Value
        }).ToList();
    }

    public async Task CreateUserSubscriptionAsync(CreateUserSubscriptionDto dto)
    {
        ValidateSubscriptionValueLength(dto.Value);

        var user = await userRepository.GetUserAsync(dto.TeamsId);

        if (user == null)
        {
            await userRepository.CreateUserAsync(dto.TeamsId);
            user = await userRepository.GetUserAsync(dto.TeamsId);
        }

        user!.Subscriptions.Add(new Subscription
        {
            Type = dto.Type,
            Value = dto.Value,
        });

        await dbCtx.SaveChangesAsync();
    }

    public async Task UpdateUserSubscriptionAsync(int id, UpdateUserSubscriptionDto dto, string teamsId)
    {
        ValidateSubscriptionValueLength(dto.NewValue);

        var subscription = await subscriptionRepository.GetSubscriptionAsync(id);

        if (subscription is null)
            throw new SubscriptionNotFoundException();

        if (teamsId is not null
            && !subscription.User.TeamsId.Equals(teamsId, StringComparison.InvariantCultureIgnoreCase))
            throw new Exception("You can't modify this subscription.");

        subscription.Value = dto.NewValue;

        await dbCtx.SaveChangesAsync();
    }

    public async Task DeleteUserSubscriptionAsync(int id)
    {
        var subscription = await subscriptionRepository.GetSubscriptionAsync(id);

        if (subscription is null)
            throw new SubscriptionNotFoundException();

        await subscriptionRepository.DeleteAsync(id);
    }

    private static void ValidateSubscriptionValueLength(string value)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value) || value.Length > 255)
            throw new ArgumentOutOfRangeException(value);
    }
}

public class SubscriptionNotFoundException() : Exception("Subscription not found");