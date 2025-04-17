using System.Text.Json;
using ADAM.API;
using ADAM.API.Extensions;
using ADAM.API.Jobs;
using ADAM.Application.Extensions;
using ADAM.Bot;
using ADAM.Common;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

Extensions.AddDbContext(builder.Services, builder.Configuration, connectionString);

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
builder.Services.AddScoped<MessageSender>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSites().AddAdamServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.RegisterAdamEndpoints();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            Status = report.Status.ToString(),
            HealthChecks = report.Entries.Select(e => new
            {
                Component = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description
            }),
            TotalDuration = report.TotalDuration
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new AllowAllConnectionsFilter()]
});

RecurringJob.AddOrUpdate<ScrapeAndNotifyJob>(
    "daily-scrape-and-notify-job",
    scraper => scraper.ExecuteAsync(),
    Cron.Daily(9, 0));

app.Run();

namespace ADAM.API
{
    public class AllowAllConnectionsFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => true;
    }
}