using ADAM.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ADAM.Common;

public static class Extensions
{
    public static void AddDbContext(IServiceCollection services, IConfiguration configuration,
        string? connectionString = null)
    {
        connectionString ??= configuration.GetConnectionString("DefaultConnection") ??
                             throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(opts => opts.UseNpgsql(connectionString));
    }
}