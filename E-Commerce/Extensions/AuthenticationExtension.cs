using E_Commerce.Application.Interfaces.Authentication;
using E_Commerce.Core.Shared.Utilties.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.JsonWebTokens;

namespace E_Commerce.Extensions;
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
     
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddSingleton< IHttpContextAccessor ,HttpContextAccessor>();    
        services.AddSingleton<TokenValidationParameters>(provider =>
        {
            var jwtConfig = provider.GetRequiredService<IOptions<JwtConfig>>().Value;
            var key = Encoding.ASCII.GetBytes(jwtConfig.Secret);

            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                //in angualr make them false but why
                //ValidateIssuer = true,   
                //ValidIssuer = jwtConfig.Issuer,
                //ValidateAudience = true,   
                //ValidAudience = jwtConfig.Audience,
                //RequireExpirationTime = true,
                //ValidateLifetime = true,
                //ClockSkew = TimeSpan.Zero
                ValidateIssuer = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtConfig.Audience,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
                
            };
        });

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; 

            })
            .AddJwtBearer(x =>

                {
                   /* x.RequireHttpsMetadata = services.BuildServiceProvider()
                                        .GetRequiredService<IWebHostEnvironment>()
                                        .IsProduction();*/
                    x.SaveToken = true;
                    x.TokenValidationParameters = services.BuildServiceProvider()
                                                             .GetRequiredService<TokenValidationParameters>();
                    x.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
                            var jti = context.Principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
                            if (await tokenService.IsTokenBlacklistedAsync(jti))
                            {
                                context.Fail("Token is blacklisted");
                            }
                        }
                    };
                }
            )
            .AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"];
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                options.CallbackPath = "/signin-google";  
                options.CorrelationCookie.SameSite = SameSiteMode.Lax; 
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always; 
                options.SaveTokens = true; 
                options.Events.OnRemoteFailure = async context =>
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        error = "Google authentication failed: User denied permissions"
                    });
                    context.HandleResponse();
                };
            }).AddCookie(); 

                
            return services;
        }
    
    
    }

