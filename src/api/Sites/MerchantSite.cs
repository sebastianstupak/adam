using ADAM.Domain.Models;
using HtmlAgilityPack;

namespace ADAM.API.Sites;

public class MerchantSite(ILogger logger) : IMerchantSite
{
    protected readonly ILogger Logger = logger;
    
    public virtual string GetUrl() => string.Empty;

    public virtual List<MerchantOffer> ExtractOffersFromPage(HtmlNode page) => [];

    public virtual async Task<List<MerchantOffer>> GetOffersAsync(CancellationToken ct)
    {
        try
        {
            var htmlDoc = await new HtmlWeb().LoadFromWebAsync(GetUrl(), ct);

            if (htmlDoc.DocumentNode is null)
            {
                Logger.LogWarning("Failed to get site from: {url}", GetUrl());
                return [];
            }

            return ExtractOffersFromPage(htmlDoc.DocumentNode);
        }
        catch (Exception ex)
        {
            Logger.LogError("Encountered an error!\n{exceptionMessage}", ex.Message);
            return [];
        }
    }
}