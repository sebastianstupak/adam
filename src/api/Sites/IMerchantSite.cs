using ADAM.Domain.Models;
using HtmlAgilityPack;

namespace ADAM.API.Sites;

/// <summary>
/// Used to implement new merchant site scalping (e.g. restaurants).
/// </summary>
public interface IMerchantSite
{
    /// <returns>The url of the website to scalp.</returns>
    string GetUrl();

    /// <summary>
    /// Per-site implementation of extracting specific elements from the retrieved HTML site.
    /// </summary>
    /// <param name="page">HtmlNode containing the body of the site to process</param>
    /// <remarks>For an example implementation, see <see cref="AuparkSite"/>.</remarks>
    /// <returns>A list of processed merchant offers ready to be persisted</returns>
    List<MerchantOffer> ExtractOffersFromPage(HtmlNode page);

    Task<List<MerchantOffer>> GetOffersAsync(CancellationToken ct);
}