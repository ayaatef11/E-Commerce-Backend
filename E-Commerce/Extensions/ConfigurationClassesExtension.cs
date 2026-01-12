using E_Commerce.Core.Shared.Settings;
using E_Commerce.Core.Shared.Utilties.Identity;

namespace E_Commerce.Extensions;
public static class ConfigurationClassesExtension
{
    public static IServiceCollection ConfigureAppSettingData(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtConfig>(configuration.GetSection("JwtConfig"));
        services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.None;
            options.Secure = CookieSecurePolicy.Always;
        });
        services.Configure<GoogleData>(configuration.GetSection("Authentication:Google"));
        services.Configure<DatabaseConnection>(configuration.GetSection("ConnectionStrings"));
        return services;
    }
}