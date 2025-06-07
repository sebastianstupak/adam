using ADAM.Domain.Models;

namespace ADAM.Application.Objects;

public class UserSubscriptions
{
    public User User { get; set; }
    public IEnumerable<Subscription> Subscriptions { get; set; }
}