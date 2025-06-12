using ADAM.Application.Objects;

namespace ADAM.Application.Sites;

/// <summary>
/// Used to implement new merchant site scalping (e.g. restaurants).
/// </summary>
public interface IMerchantSite
{
    /// <returns>The url of the website to scalp.</returns>
    string GetUrl();

    /// <summary>
    /// Retrieves the content of the site provided by <see cref="GetUrl"/> and processes it as per implementation.
    /// </summary>
    /// <returns>A list of </returns>
    Task<SiteMerchantOffers> GetOffersAsync(CancellationToken ct);
}