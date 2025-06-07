using System.ComponentModel.DataAnnotations;

namespace ADAM.Domain.Models;

public class MerchantOffer
{
    public long Id { get; set; }

    [MaxLength(255)]
    public required string MerchantName { get; set; }

    public required string Meal { get; set; }

    public decimal? Price { get; set; }

    public TimestampedHtmlRecord HtmlRecord { get; set; }

    public required DateTime CreationDate { get; set; }
}