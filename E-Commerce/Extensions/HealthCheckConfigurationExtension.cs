using E_Commerce.Core.Shared.Settings;

namespace E_Commerce.Extensions;
public static class HealthCheckConfigurations
{
    public static IServiceCollection AddHealthCheckConfigurations(this IServiceCollection services, DatabaseConnection connections)
    {
        services.AddHealthChecks()
            .AddSqlServer(connections.DefaultConnection, name: "StoreDb-check")
            .AddRedis(connections.RedisConnection, name: "Redis-check")
            .AddHangfire(t => t.MinimumAvailableServers = 1, name: "Hangfire-check");
        return services;
    }
}