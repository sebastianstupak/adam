using ADAM.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace ADAM.IntegrationTests.TestConfiguration;

public sealed class TestWebAppFactory : WebApplicationFactory<Program>, IAsyncInitializer
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .Build();
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        CreateDb();
    }

    public override async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        var connectionString = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            PersistSecurityInfo = true
        }.ConnectionString;

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(
                opts => opts.UseNpgsql(connectionString)
            );
        });
    }

    private void CreateDb()
    {
        using var scope = Services.CreateScope();
        var dbCtx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbCtx.Database.EnsureCreated();
    }
}