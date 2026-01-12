namespace E_Commerce.Middlewares;
public class ApiKeyRateLimitingMiddleware(RequestDelegate next, IMemoryCache cache)
{
    private readonly RequestDelegate _next = next;
    private readonly IMemoryCache _cache = cache;

    public async Task InvokeAsync(HttpContext context)
    {
        var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();

        if (apiKey != null)
        {
            var cacheKey = $"apiKey_{apiKey}";

            if (!_cache.TryGetValue(cacheKey, out int requestCount))
            {
                requestCount = 0;
            }

            if (requestCount >= 100) // Limit to 100 requests
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("API rate limit exceeded");
                return;
            }

            _cache.Set(cacheKey, requestCount + 1, TimeSpan.FromMinutes(1));
        }

        await _next(context);
    }
}