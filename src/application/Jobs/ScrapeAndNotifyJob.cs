using System.Collections.Concurrent;
using ADAM.Application.Services;
using ADAM.Application.Services.Users;
using ADAM.Application.Sites;
using ADAM.Domain;
using ADAM.Domain.Models;
using ADAM.Domain.Repositories.Users;
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
            var merchantOffers = new ConcurrentBag<MerchantOffer>();

            await Parallel.ForEachAsync(_merchantSites,
                async (site, ct) =>
                {
                    try
                    {
                        logger.LogInformation("Starting web scraping for URL: {Url}", site.GetUrl());

                        var offers = await site.GetOffersAsync(ct);
                        foreach (var offer in offers)
                            merchantOffers.Add(offer);

                        logger.LogInformation("Web scraping completed successfully for URL: {Url}", site.GetUrl());
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occured while scraping: {URL}\n{exMsg}", site.GetUrl(),
                            ex.Message);
                    }
                }
            );

            var merchantNamesAndMeals = new List<string>();
            merchantNamesAndMeals.AddRange(merchantOffers.Select(mo => mo.Meal));
            merchantNamesAndMeals.AddRange(merchantOffers.Select(mo => mo.Name));

            var tuples = await _userService.GetUsersWithMatchingSubscriptionsAsync(merchantNamesAndMeals);
            foreach (var tuple in tuples)
                await _messageSender.SendMessageToUserAsync(
                    _configuration["BotId"] ?? throw new Exception("NULL BOT ID"),
                    tuple.u.TeamsId,
                    tuple.message
                );

            dbCtx.AddRange(merchantOffers);
            await dbCtx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured: {exMsg}\n{exInnerEx}", ex.Message, ex.InnerException);
        }
    }
}