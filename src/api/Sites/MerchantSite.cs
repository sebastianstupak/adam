using ADAM.Domain.Models;
using HtmlAgilityPack;

namespace ADAM.API.Sites;

public class MerchantSite : IMerchantSite
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:135.0) Gecko/20100101 Firefox/135.0";
    const string Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
    const string AcceptEncoding = "gzip,deflate,br";

    public MerchantSite(
        HttpClient httpClient,
        ILogger logger
    )
    {
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        httpClient.DefaultRequestHeaders.Accept.ParseAdd(Accept);
        httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd(AcceptEncoding);
    }

    public virtual string GetUrl() => string.Empty;

    public virtual List<MerchantOffer> ExtractOffersFromPage(HtmlNode page) => [];

    public virtual async Task<List<MerchantOffer>> GetOffersAsync()
    {
        try
        {
            var htmlDoc = await new HtmlWeb().LoadFromWebAsync(GetUrl());

            if (htmlDoc.DocumentNode is null)
            {
                _logger.LogWarning("Failed to get site from: {url}", GetUrl());
                return [];
            }

            return ExtractOffersFromPage(htmlDoc.DocumentNode);
        }
        catch (Exception ex)
        {
            _logger.LogError("Encountered an error!\n{exceptionMessage}", ex.Message);
            return [];
        }
    }
}