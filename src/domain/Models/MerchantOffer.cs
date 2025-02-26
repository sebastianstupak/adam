using System.ComponentModel.DataAnnotations;

namespace ADAM.Domain.Models;

public class MerchantOffer
{
    public long Id { get; set; }
    
    [MaxLength(255)]
    public required string Name { get; set; }
    
    public required string Meal { get; set; }
    
    public required decimal Price { get; set; }
    
    public required string Html { get; set; }
    
    public required DateTime CreationDate { get; set; }
}