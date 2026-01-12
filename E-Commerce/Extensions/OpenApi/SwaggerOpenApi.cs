using Swashbuckle.AspNetCore.SwaggerGen;
namespace E_Commerce.Extensions.OpenApi;
public static class SwaggerOpenAPI
{
    public static void ConfigureAppSwaggerGen(this IServiceCollection services, OpenApiInfo appApiInfo, bool userApiKeyAuthorizeFilter = false,
        bool useTokenAuthorizeFilter = false)
    {
        services.AddSwaggerGen(c =>
        {
            ConfigureSwagger(c, appApiInfo, userApiKeyAuthorizeFilter, useTokenAuthorizeFilter);
        });
    }

    private static void ConfigureSwagger(SwaggerGenOptions options, OpenApiInfo appApiInfo,
        bool userApiKeyAuthorizeFilter = false,  bool useTokenAuthorizeFilter = false)
    {

        options.SwaggerDoc("v1", appApiInfo);

        if (userApiKeyAuthorizeFilter)
        {
            var securitySchemeApiKey = new OpenApiSecurityScheme
            {
                Name = "X-Api-Key",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "apiKey",
                In = ParameterLocation.Header,
                Description = "API Key required for access to this endpoint"
            };

            var securityRequirementApiKey = new OpenApiSecurityRequirement
                {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "ApiKey"
                                },
                                Scheme = "apiKey",
                                Name = "X-Api-Key",
                                In = ParameterLocation.Header
                            },
                            new string[] { }
                    }
                };

            options.AddSecurityDefinition("ApiKey", securitySchemeApiKey);
            options.AddSecurityRequirement(securityRequirementApiKey);
        }

        if (useTokenAuthorizeFilter)
        {
            var securitySchemeBearer = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "Bearer token required for access to this endpoint"
            };

            var securityRequirementBearer = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "bearer",
                            Name = "Authorization",
                            In = ParameterLocation.Header
                        },
                        new string[] { }
                    }
                };

            options.AddSecurityDefinition("Bearer", securitySchemeBearer);
            options.AddSecurityRequirement(securityRequirementBearer);
        }
    }
}