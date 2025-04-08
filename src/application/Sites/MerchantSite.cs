using ADAM.Domain.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace ADAM.Application.Sites;

public class MerchantSite(ILogger<MerchantSite> logger) : IMerchantSite
{
    protected readonly ILogger Logger = logger;
    
    public virtual string GetUrl() => string.Empty;

    /// <summary>
    /// Per-site implementation of extracting specific elements from the retrieved HTML site.
    /// </summary>
    /// <param name="page">HtmlNode containing the body of the site to process</param>
    /// <remarks>For an example implementation, see <see cref="AuparkSite"/>.</remarks>
    /// <returns>A list of processed merchant offers ready to be persisted</returns>
    protected virtual List<MerchantOffer> ExtractOffersFromPage(HtmlNode page) => [];

    public virtual async Task<IEnumerable<MerchantOffer>> GetOffersAsync(CancellationToken ct)
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