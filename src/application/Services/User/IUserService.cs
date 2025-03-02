using ADAM.Domain.Models;

namespace ADAM.Application.Services.User;

public interface IUserService
{
    Task<List<Subscription>> GetUserSubscriptionsAsync(Guid userGuid);
}