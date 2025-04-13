namespace ADAM.Domain.Models;

public class User
{
    public long Id { get; set; }

    public required string TeamsId { get; set; }

    public required DateTime CreationDate { get; set; }

    public virtual List<Subscription> Subscriptions { get; set; } = [];
}