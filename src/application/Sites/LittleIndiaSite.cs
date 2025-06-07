using ADAM.Domain.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace ADAM.Application.Sites;

public class LittleIndiaSite(ILogger<MerchantSite> logger) : MerchantSite(logger)
{
    private const string LittleIndiaSiteUrl = "http://www.littleindiakosice.sk";

    public override string GetUrl() => LittleIndiaSiteUrl;

    protected override List<MerchantOffer> ExtractOffersFromPage(HtmlNode page)
    {
        var offers = new List<MerchantOffer>();

        var menuContainer = page.SelectSingleNode("//div[contains(@class, 'container denneMenu')]");

        if (menuContainer is null)
        {
            Logger.LogWarning("Menu container not found");
            return [];
        }

        var menuItems = menuContainer.SelectNodes(".//div[@class='item']");

        if (menuItems is null)
        {
            Logger.LogWarning("No menu items found");
            return [];
        }

        foreach (var item in menuItems)
        {
            offers.AddRange(CreateMerchantOffers(page, item));
        }

        return offers;
    }

    private static List<MerchantOffer> CreateMerchantOffers(HtmlNode page, HtmlNode menuItem)
    {
        var offers = new List<MerchantOffer>();
        var utcNow = DateTime.UtcNow;

        var categoryNode = menuItem.SelectSingleNode(".//h6");
        var mealNode = menuItem.SelectSingleNode(".//div[@class='fName']");

        if (categoryNode is null || mealNode is null)
            return offers;

        var category = categoryNode.InnerText.Trim();
        var mealsText = mealNode.InnerText.Trim();

        var individualMeals = mealsText.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(m => m.Trim())
            .Where(m => !string.IsNullOrEmpty(m));

        offers.AddRange(
            individualMeals.Select(meal => new MerchantOffer
                {
                    MerchantName = "Little India",
                    Meal = $"{category}: {meal}",
                    Price = null,
                    CreationDate = utcNow
                }
            )
        );

        return offers;
    }
}