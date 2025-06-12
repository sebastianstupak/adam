using System.Threading.RateLimiting;

namespace ADAM.API.Extensions;

public static class RateLimiterExtensions
{
    public static IServiceCollection AddCommonRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        int permitLimit = configuration.GetValue<int?>("RateLimiting:PermitLimit") ?? 75;
        int windowSeconds = configuration.GetValue<int?>("RateLimiting:WindowSeconds") ?? 60;
        
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromSeconds(windowSeconds)
                    }));
        });

        return services;
    }
}
