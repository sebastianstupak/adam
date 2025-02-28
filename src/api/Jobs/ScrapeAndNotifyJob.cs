using System.Collections.Concurrent;
using ADAM.Application.Sites;
using ADAM.Domain;
using ADAM.Domain.Models;

namespace ADAM.API.Jobs;

public class ScrapeAndNotifyJob(ILogger<ScrapeAndNotifyJob> logger, AppDbContext dbCtx, IEnumerable<IMerchantSite> merchantSites) : IJob
{
    private readonly IList<IMerchantSite> _merchantSites = merchantSites.ToList();

    public async Task ExecuteAsync()
    {
        try
        {
            var output = new ConcurrentBag<MerchantOffer>();

            await Parallel.ForEachAsync(_merchantSites,
                async (site, ct) =>
                {
                    try
                    {
                        logger.LogInformation("Starting web scraping for URL: {Url}", site.GetUrl());

                        var offers = await site.GetOffersAsync(ct);
                        foreach (var offer in offers)
                            output.Add(offer);

                        logger.LogInformation("Web scraping completed successfully for URL: {Url}", site.GetUrl());
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occured while scraping: {URL}\n{exMsg}", site.GetUrl(), ex.Message);
                    }
                });

            dbCtx.AddRange(output);
            await dbCtx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured: {exMsg}\n{exInnerEx}", ex.Message, ex.InnerException);
        }
    }
}