using HtmlAgilityPack;

namespace ADAM.API.Jobs;

public class ScrapeAndNotifyJob : IJob
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ScrapeAndNotifyJob> _logger;

    public ScrapeAndNotifyJob(
        ILogger<ScrapeAndNotifyJob> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task ExecuteAsync()
    {
        var url = _configuration["ProcessedUrl"];

        try
        {
            _logger.LogInformation("Starting web scraping for URL: {Url}", url);

            // Download the HTML content
            var html = await _httpClient.GetStringAsync(url);

            // Parse HTML using HtmlAgilityPack
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            // Example: Extract all paragraph texts
            var paragraphs = htmlDoc.DocumentNode.SelectNodes("//p");
            var extractedContent = paragraphs != null
                ? string.Join("\n", paragraphs.Select(p => p.InnerText))
                : "No content found";

            /*

            // Store the scraped data
            var scrapedData = new ScrapedData
            {
                Url = url,
                Content = extractedContent,
                ScrapedAt = DateTime.UtcNow
            };

            _dbContext.ScrapedData.Add(scrapedData);
            await _dbContext.SaveChangesAsync();
            */

            _logger.LogInformation("Web scraping completed successfully for URL: {Url}", url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while scraping URL: {Url}", url);
            throw;
        }
    }
}
