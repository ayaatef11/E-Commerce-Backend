using E_Commerce.Infrastructure.Providers;

namespace Extensions;
public static class IdentityExtension
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.SignIn.RequireConfirmedEmail = true; 
                options.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
                options.Tokens.ProviderMap.Add("Email", new TokenProviderDescriptor(typeof(EmailTokenProvider<AppUser>)));
                options.Tokens.ProviderMap.Add("Phone", new TokenProviderDescriptor(typeof(PhoneNumberTokenProvider<AppUser>)));
                options.Tokens.ProviderMap.Add("Authenticator", new TokenProviderDescriptor(typeof(AuthenticatorTokenProvider<AppUser>)));

            })
    .AddEntityFrameworkStores<StoreContext>()
    .AddDefaultTokenProviders().AddTokenProvider<DigitsEmailProvider<AppUser>>("emailconfirmation");

            return services;
        }

    }

