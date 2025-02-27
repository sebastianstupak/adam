using ADAM.Domain.Models;
using HtmlAgilityPack;

namespace ADAM.API.Sites;

public class AuparkSite(ILogger logger) : MerchantSite(logger)
{
    private const string AuparkSiteUrl = "https://www.auparkkosice.sk/obedove-menu";

    public override string GetUrl() => AuparkSiteUrl;

    public override List<MerchantOffer> ExtractOffersFromPage(HtmlNode page)
    {
        var offers = new List<MerchantOffer>();

        // Get all sections
        var sectionNodes = page.SelectNodes("//div[contains(@id, '-section')]");

        if (sectionNodes == null)
        {
            Logger.LogWarning("No sections found with IDs ending in '-section'");
            return [];
        }

        foreach (var node in sectionNodes)
        {
            var id = node.GetAttributeValue("id", "");
            var merchantName = FormatSectionName(id.Replace("-section", ""));

            // Extract official name if possible
            var nameNode = node.SelectSingleNode(".//div[contains(@class, 'font-normal text-base')]");
            if (nameNode is not null)
                merchantName = nameNode.InnerText.Trim();

            // Find all menu items in this section
            var menuItems = node.SelectNodes(".//div[contains(@class, 'flex gap-8 items-start border-b')]");

            if (menuItems is null)
                continue;

            foreach (var menuItem in menuItems)
            {
                var mealNode = menuItem.SelectSingleNode(".//div[contains(@class, 'w-full text-black')]");
                var priceNode =
                    menuItem.SelectSingleNode(".//div[contains(@class, 'w-[120px] text-right font-semibold')]");

                if (mealNode is null || priceNode is null)
                    continue;

                var meal = mealNode.InnerText.Trim();
                var priceText = priceNode.InnerText.Trim().Replace("€", "").Trim();

                // Handle price format irregularities
                decimal price = 0;
                if (!string.IsNullOrEmpty(priceText))
                {
                    // Replace comma with period for decimal parsing if needed
                    priceText = priceText.Replace(',', '.');
                    if (decimal.TryParse(priceText,
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out var parsedPrice))
                    {
                        price = parsedPrice;
                    }
                }

                var utcNow = DateTime.UtcNow;
                var offer = new MerchantOffer
                {
                    Name = merchantName,
                    Meal = meal,
                    Price = price,
                    Html = page.InnerHtml,
                    CreationDate = utcNow
                };

                offers.Add(offer);
            }
        }

        return offers;
    }

    /// <summary>
    /// Formats Aupark's section names to friendlier names.
    /// </summary>
    /// <example>tahiti-section --> Tahiti</example>
    /// <returns></returns>
    private string FormatSectionName(string sectionId)
    {
        var words = sectionId.Split('-');
        for (int i = 0; i < words.Length; i++)
        {
            if (!string.IsNullOrEmpty(words[i]))
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            }
        }

        return string.Join(" ", words);
    }
}