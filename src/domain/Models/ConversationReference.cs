namespace ADAM.Domain.Models;

/// <summary>
/// Tracks conversation references for proactive communication with known users.
/// </summary>
public class ConversationReference
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string ActivityId { get; set; }
    public string ChannelId { get; set; }
    public string ServiceUrl { get; set; }

    public virtual User User { get; set; }
}