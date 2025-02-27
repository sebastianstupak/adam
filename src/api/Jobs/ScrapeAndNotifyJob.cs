using ADAM.API.Sites;
using ADAM.Domain;

namespace ADAM.API.Jobs;

public class ScrapeAndNotifyJob : IJob
{
    private readonly ILogger _logger;
    private readonly AppDbContext _dbCtx;

    private readonly IList<IMerchantSite> _merchantSites = [];

    public ScrapeAndNotifyJob(ILogger logger, AppDbContext dbCtx)
    {
        _logger = logger;
        _dbCtx = dbCtx;
    }

    public async Task ExecuteAsync()
    {
        // TODO: Add (x)Site objects to _merchantSites
        
        try
        {
            await Parallel.ForEachAsync(_merchantSites, async (site, ct) =>
            {
                try
                {
                    _logger.LogInformation("Starting web scraping for URL: {Url}", site.GetUrl());

                    var offers = await site.GetOffersAsync(ct);
                    _dbCtx.AddRange(offers);

                    _logger.LogInformation("Web scraping completed successfully for URL: {Url}", site.GetUrl());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while scraping: {URL}", site.GetUrl());
                }
            });

            await _dbCtx.SaveChangesAsync();
            
            // TODO: Clear _merchantSites
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured: {exMsg}\n{exInnerEx}", ex.Message, ex.InnerException);
        }
    }
}