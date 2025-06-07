using ADAM.Domain.Models;

namespace ADAM.Application.Objects;

public class SiteMerchantOffers
{
    public string SiteHtml { get; set; }
    public IEnumerable<MerchantOffer> Offers { get; set; } = new List<MerchantOffer>();
}