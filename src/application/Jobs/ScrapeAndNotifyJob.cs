using System.Collections.Concurrent;
using ADAM.Application.Services;
using ADAM.Application.Services.Users;
using ADAM.Application.Sites;
using ADAM.Domain;
using ADAM.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ADAM.Application.Jobs;

public class ScrapeAndNotifyJob(
    ILogger<ScrapeAndNotifyJob> logger,
    AppDbContext dbCtx,
    IUserService userService,
    IEnumerable<IMerchantSite> merchantSites,
    MessageSendingService messageSender,
    IConfiguration configuration
) : IJob
{
    private readonly IUserService _userService = userService;
    private readonly MessageSendingService _messageSender = messageSender;
    private readonly IConfiguration _configuration = configuration;
    private readonly IList<IMerchantSite> _merchantSites = merchantSites.ToList();

    public async Task ExecuteAsync()
    {
        try
        {
            var botId = _configuration["BotId"] ?? throw new Exception("No bot ID present in configuration");
            var merchantOffers = new ConcurrentBag<MerchantOffer>();

            await Parallel.ForEachAsync(_merchantSites,
                async (site, ct) =>
                {
                    try
                    {
                        logger.LogInformation("Starting web scraping for URL: {Url}", site.GetUrl());

                        var siteOffers = await site.GetOffersAsync(ct);

                        var htmlRecord = new TimestampedHtmlRecord
                        {
                            Url = site.GetUrl(),
                            HtmlContent = siteOffers.SiteHtml,
                            CreationDate = DateTime.UtcNow
                        };

                        dbCtx.TimestampedHtmlRecords.Add(htmlRecord);

                        foreach (var offer in siteOffers.Offers)
                        {
                            offer.HtmlRecord = htmlRecord;
                            merchantOffers.Add(offer);
                        }

                        logger.LogInformation("Web scraping completed successfully for URL: {Url}", site.GetUrl());
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occured while scraping: {URL}\n{exMsg}", site.GetUrl(),
                            ex.Message);
                    }
                }
            );

            var mealSubs = await _userService.GetUsersWithMatchingSubscriptionsAsync(
                merchantOffers.Select(mo => mo.Meal)
            );
            var merchantSubs = await _userService.GetUsersWithMatchingSubscriptionsAsync(
                merchantOffers.Select(mo => mo.MerchantName)
            );

            if (mealSubs.Any() || merchantSubs.Any())
            {
                await _messageSender.SendCombinedNotificationAsync(
                    botId,
                    mealSubs,
                    merchantSubs,
                    merchantOffers
                );
            }

            dbCtx.AddRange(merchantOffers);
            await dbCtx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured: {exMsg}\n{exInnerEx}", ex.Message, ex.InnerException);
        }
    }
}