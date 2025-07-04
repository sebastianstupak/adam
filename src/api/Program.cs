using System.Text.Json;
using ADAM.API;
using ADAM.API.Extensions;
using ADAM.Application.Extensions;
using ADAM.Application.Jobs;
using ADAM.Domain;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SQLite;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();

builder.Services.AddBotFramework();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<AppDbContext>(opts => opts.UseSqlite(connectionString));

    builder.Services.AddHealthChecks()
        .AddSqlite(connectionString, name: "database")
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSQLiteStorage(connectionString));

    builder.Services.AddHangfireServer();
}

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services
    .AddSites()
    .AddAdamServices()
    .AddCommonRateLimiter(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();

app.Use(async (context, next) =>
{
    if (context.Request.IsHttps && context.Request.Path.StartsWithSegments("/api/messages"))
    {
        var url = "http://" + context.Request.Host + context.Request.Path +
                  context.Request.QueryString;

        context.Response.Redirect(url);

        return;
    }

    await next();
});

app.RegisterAdamEndpoints();

if (!app.Environment.IsEnvironment("test"))
{
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
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.EnsureCreatedAsync();
    await context.Database.MigrateAsync();
}

app.Run();

namespace ADAM.API
{
    public class AllowAllConnectionsFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => true;
    }
}

// For tests
public partial class Program;