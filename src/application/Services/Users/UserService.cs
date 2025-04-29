using System.Collections;
using System.Diagnostics;
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

    public async Task CreateUserAsync(string teamsId)
    {
        await _userRepository.CreateUserAsync(teamsId);
    }

    public async Task<IEnumerable<GetUserSubscriptionDto>> GetUserSubscriptionsAsync(string teamsId)
    {
        var user = await _userRepository.GetUserAsync(teamsId);

        if (user is null)
            await CreateUserAsync(teamsId);

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
            await CreateUserAsync(dto.TeamsId);
            user = await _userRepository.GetUserAsync(dto.TeamsId);
        }

        user!.Subscriptions.Add(new Subscription
        {
            Type = dto.Type,
            Value = dto.Value,
        });

        await _dbCtx.SaveChangesAsync();
    }

    public async Task UpdateUserSubscriptionAsync(int id, UpdateUserSubscriptionDto dto, string teamsId)
    {
        ValidateSubscriptionValueLength(dto.NewValue);

        var subscription = await _subscriptionRepository.GetSubscriptionAsync(id);

        if (subscription is null)
            throw new SubscriptionNotFoundException();

        if (teamsId is not null
            && !subscription.User.TeamsId.Equals(teamsId, StringComparison.InvariantCultureIgnoreCase))
            throw new Exception("You can't modify this subscription.");

        subscription.Value = dto.NewValue;

        await _dbCtx.SaveChangesAsync();
    }

    public async Task DeleteUserSubscriptionAsync(int id)
    {
        var subscription = await _subscriptionRepository.GetSubscriptionAsync(id);

        if (subscription is null)
            throw new SubscriptionNotFoundException();

        await _subscriptionRepository.DeleteAsync(id);
    }

    public async Task<bool> DidUserAcceptDataStorageAsync(string teamsId)
    {
        var user = await _dbCtx.Users.FirstOrDefaultAsync(
            u => u.TeamsId == teamsId
        );

        return user is not null && user.AcceptsDataStorage;
    }

    public async Task UpdateUserConsentAsync(string teamsId)
    {
        var user = await _userRepository.GetUserAsync(teamsId);

        if (user is null)
            await CreateUserAsync(teamsId);

        user = await _userRepository.GetUserAsync(teamsId);

        user!.AcceptsDataStorage = true;
        await _dbCtx.SaveChangesAsync();
    }

    public async Task<IEnumerable<(User u, string message)>> GetUsersWithMatchingSubscriptionsAsync(
        List<string> merchantNamesAndMeals)
    {
        var tuples = await _userRepository.GetUsersWithMatchingSubscriptionsAsync(merchantNamesAndMeals);
        List<(User User, string message)> output = [];

        foreach (var tuple in tuples)
        {
            var matchingCompanies = merchantNamesAndMeals.Where(
                mnam => tuple.subscriptions.Any(
                    s => s.Type is SubscriptionType.Merchant
                         && (s.Value.Equals(mnam, StringComparison.InvariantCultureIgnoreCase)
                             || mnam.Contains(s.Value, StringComparison.InvariantCultureIgnoreCase))
                )
            ).Distinct();

            var matchingOffers = merchantNamesAndMeals.Where(
                mnam => tuple.subscriptions.Any(
                    s => s.Type is SubscriptionType.Offer
                         && (s.Value.Equals(mnam, StringComparison.InvariantCultureIgnoreCase)
                             || mnam.Contains(s.Value, StringComparison.InvariantCultureIgnoreCase))
                )
            );

            var message = $"""
                           🍔 Hey! I found you something to eat.

                           # Companies
                           {string.Join(", ", matchingCompanies)}

                           # Food
                           {string.Join("\n\n", matchingOffers)}
                           """;

            output.Add((tuple.user, message));
        }

        return output;
    }

    private static void ValidateSubscriptionValueLength(string value)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value) || value.Length > 255)
            throw new ArgumentOutOfRangeException(value);
    }
}

public class SubscriptionNotFoundException() : Exception("Subscription not found");