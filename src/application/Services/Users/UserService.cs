using ADAM.Application.Objects;
using ADAM.Domain;
using ADAM.Domain.Models;
using ADAM.Domain.Repositories.Subscriptions;
using ADAM.Domain.Repositories.Users;
using Microsoft.EntityFrameworkCore;

namespace ADAM.Application.Services.Users;

public class UserService(
    ISubscriptionRepository subscriptionRepository,
    IUserRepository userRepository,
    AppDbContext dbCtx
) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ISubscriptionRepository _subscriptionRepository = subscriptionRepository;
    private readonly AppDbContext _dbCtx = dbCtx;

    public async Task CreateUserAsync(string teamsId, string name)
    {
        await _userRepository.CreateUserAsync(teamsId, name);
    }

    public async Task<IEnumerable<GetUserSubscriptionDto>> GetUserSubscriptionsAsync(string teamsId)
    {
        var user = await _userRepository.GetUserAsync(teamsId);

        if (user is null)
            throw new UserNotFoundException();

        var subscriptions = await _subscriptionRepository.GetSubscriptionsAsync(teamsId);

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

        var user = await _userRepository.GetUserAsync(dto.TeamsId);

        if (user is null)
        {
            await CreateUserAsync(dto.TeamsId, dto.Name);
            user = await _userRepository.GetUserAsync(dto.TeamsId);
        }

        if (user!.Subscriptions.Where(s => s.Type == dto.Type)
            .Any(s => s.Value.Equals(dto.Value, StringComparison.InvariantCultureIgnoreCase)))
            throw new InvalidOperationException("A subscription with this value already exists.");

        user.Subscriptions.Add(new Subscription
        {
            Type = dto.Type,
            Value = dto.Value,
        });

        await _dbCtx.SaveChangesAsync();
    }

    public async Task UpdateUserSubscriptionAsync(int id, UpdateUserSubscriptionDto dto, string teamsId)
    {
        ValidateSubscriptionValueLength(dto.NewValue);

        var subscription = await _subscriptionRepository.GetSubscriptionAsync(id)
                           ?? throw new SubscriptionNotFoundException();

        if (!subscription.User.TeamsId.Equals(teamsId, StringComparison.InvariantCultureIgnoreCase))
            throw new UnauthorizedAccessException("You can't update this subscription.");

        subscription.Value = dto.NewValue;

        await _dbCtx.SaveChangesAsync();
    }

    public async Task DeleteUserSubscriptionAsync(long id, string teamsId)
    {
        var subscription = await _subscriptionRepository.GetSubscriptionAsync(id)
                           ?? throw new SubscriptionNotFoundException();

        if (!subscription.User.TeamsId.Equals(teamsId, StringComparison.InvariantCultureIgnoreCase))
            throw new UnauthorizedAccessException("You can't delete this subscription.");

        await _subscriptionRepository.DeleteAsync(id);
    }

    public async Task<bool> DidUserAcceptDataStorageAsync(string teamsId)
    {
        var user = await _dbCtx.Users.FirstOrDefaultAsync(u => u.TeamsId == teamsId
        );

        return user is not null && user.AcceptsDataStorage;
    }

    public async Task UpdateUserConsentAsync(string teamsId, string name)
    {
        var user = await _userRepository.GetUserAsync(teamsId);

        if (user is null)
            await CreateUserAsync(teamsId, name);

        user = await _userRepository.GetUserAsync(teamsId);

        user!.AcceptsDataStorage = true;
        await _dbCtx.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserSubscriptions>> GetUsersWithMatchingSubscriptionsAsync(
        IEnumerable<string> valuesToMatchAgainst)
    {
        return (
            await _userRepository.GetUsersWithMatchingSubscriptionsAsync(valuesToMatchAgainst)
        ).Select(tuple =>
            new UserSubscriptions
            {
                User = tuple.user,
                Subscriptions = tuple.subscriptions
            }
        );
    }

    private static void ValidateSubscriptionValueLength(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 255)
            throw new Exception("Missing or invalid value, or value longer than 255 characters.");
    }
}

public class SubscriptionNotFoundException() : Exception("Subscription not found");

public class UserNotFoundException() : Exception("User not found");