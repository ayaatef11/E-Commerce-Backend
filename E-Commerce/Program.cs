using Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCommonServices(builder.Configuration);
    
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.ConfigureAppSettingData(builder.Configuration);
builder.Services.AddFluentValidation();
builder.Services.AddRateLimitingConfigurations();

builder.Services.ConfigureAppSwaggerGen(
    appApiInfo: new OpenApiInfo
    {
        Title = "Causmatic Store",
        Version = "v1", 
    },
    userApiKeyAuthorizeFilter: true,  
    useTokenAuthorizeFilter:  true     
);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = long.MaxValue;
});
var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseDefaultFiles();

app.UseStaticFiles();

app.UseCookiePolicy();

app.UseRequestLocalization();

app.UseRouting();


app.UseCors(policy =>
{
    policy.AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials()
           .SetIsOriginAllowed(_ => true);
});

app.Use(async (context, next) =>
{
    context.Response.Headers["Cross-Origin-Opener-Policy"] = "unsafe-none";
    context.Response.Headers["Cross-Origin-Embedder-Policy"] = "unsafe-none";
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.UseResponseCaching();

app.MapControllers();
app.MapFallbackToFile("index.html");
app.Run();