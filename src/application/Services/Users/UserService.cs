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
    public async Task<IEnumerable<GetUserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userGuid)
    {
        var user = await userRepository.GetUserAsync(userGuid);

        if (user == null)
            await userRepository.CreateUserAsync(userGuid);

        var subscriptions = await subscriptionRepository.GetSubscriptionsAsync(userGuid);

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

        var user = await userRepository.GetUserAsync(dto.UserGuid);

        if (user == null)
        {
            await userRepository.CreateUserAsync(dto.UserGuid);
            user = await userRepository.GetUserAsync(dto.UserGuid);
        }

        user!.Subscriptions.Add(new Subscription
        {
            Type = dto.Type,
            Value = dto.Value,
        });

        await dbCtx.SaveChangesAsync();
    }

    public async Task UpdateUserSubscriptionAsync(int id, UpdateUserSubscriptionDto dto)
    {
        ValidateSubscriptionValueLength(dto.NewValue);

        var subscription = await subscriptionRepository.GetSubscriptionAsync(id);

        if (subscription is null)
            throw new SubscriptionNotFoundException();

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

    private void ValidateSubscriptionValueLength(string value)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value) || value.Length > 255)
            throw new ArgumentOutOfRangeException(value);
    }
}

public class SubscriptionNotFoundException() : Exception("Subscription not found");