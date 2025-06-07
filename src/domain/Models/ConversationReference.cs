namespace ADAM.Domain.Models;

/// <summary>
/// Tracks conversation references for proactive communication.
/// </summary>
public class ConversationReference
{
    public long Id { get; set; }
    public string ServiceUrl { get; set; }
    public string ConversationId { get; set; }
}