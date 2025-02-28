using System.ComponentModel.DataAnnotations;

namespace ADAM.Domain.Models;

public class User
{
    public long Id { get; set; }
    
    public Guid Guid { get; set; }

    [MaxLength(255)]
    public required string Name { get; set; }

    public required DateTime CreationDate { get; set; }

    public virtual List<Subscription> Subscriptions { get; set; } = [];
}