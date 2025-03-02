using ADAM.Domain.Models;

namespace ADAM.Application.Objects;

public class GetUserSubscriptionDto
{
    public required long Id { get; set; }

    public required SubscriptionType Type { get; set; }

    public required string Value { get; set; }
}