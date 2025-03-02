using System.ComponentModel.DataAnnotations;

namespace ADAM.Domain.Models;

public class Subscription
{
    public long Id { get; set; }

    public SubscriptionType Type { get; set; }

    [MaxLength(255)]
    public required string Value { get; set; }

    public virtual User User { get; set; }
}

/// <summary>
/// Determines which part of the outcome of a scraping a user is subscribed to. 
/// </summary>
public enum SubscriptionType
{
    /// <summary>
    /// Merchant name subscription
    /// </summary>
    Merchant,

    /// <summary>
    /// Merchant offer subscription
    /// </summary>
    Offer
}