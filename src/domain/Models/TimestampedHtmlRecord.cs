namespace ADAM.Domain.Models;

public class TimestampedHtmlRecord
{
    public long Id { get; set; }

    public string Url { get; set; }

    public string HtmlContent { get; set; }

    public DateTime CreationDate { get; set; }
}