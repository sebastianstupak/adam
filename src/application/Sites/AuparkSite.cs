using ADAM.Domain.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace ADAM.Application.Sites;

public class AuparkSite(ILogger<AuparkSite> logger) : MerchantSite(logger)
{
    private const string AuparkSiteUrl = "https://www.auparkkosice.sk/obedove-menu";

    public override string GetUrl() => AuparkSiteUrl;

    protected override List<MerchantOffer> ExtractOffersFromPage(HtmlNode page)
    {
        var offers = new List<MerchantOffer>();

        // Get all sections
        var sectionNodes = page.SelectNodes("//div[contains(@id, '-section')]");

        if (sectionNodes is null)
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

            // Create and collect merchant offers
            offers.AddRange(
                menuItems.Select(menuItem => CreateMerchantOffer(menuItem, merchantName)
                ).OfType<MerchantOffer>());
        }

        return offers;
    }

    private static MerchantOffer? CreateMerchantOffer(HtmlNode menuItem, string merchantName)
    {
        var utcNow = DateTime.UtcNow;

        var mealNode = menuItem.SelectSingleNode(".//div[contains(@class, 'w-full text-black')]");
        var priceNode =
            menuItem.SelectSingleNode(".//div[contains(@class, 'w-[120px] text-right font-semibold')]");

        if (mealNode is null || priceNode is null)
            return null;

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

        return new MerchantOffer
        {
            MerchantName = merchantName,
            Meal = meal,
            Price = price,
            CreationDate = utcNow
        };
    }

    /// <summary>
    /// Formats Aupark's section names to friendlier names.
    /// </summary>
    /// <example>tahiti-section --> Tahiti</example>
    private static string FormatSectionName(string sectionId)
    {
        var words = sectionId.Split('-');
        for (var i = 0; i < words.Length; i++)
        {
            if (!string.IsNullOrEmpty(words[i]))
            {
                words[i] = char.ToUpper(words[i][0]) + words[i][1..];
            }
        }

        return string.Join(" ", words);
    }
}