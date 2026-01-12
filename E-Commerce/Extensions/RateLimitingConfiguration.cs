namespace E_Commerce.Extensions;
public static class RateLimitingConfigurations
{
    public static IServiceCollection AddRateLimitingConfigurations(this IServiceCollection services)
    {
        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.AddFixedWindowLimiter("FixedWindowPolicy", options =>
            {
                options.Window = TimeSpan.FromSeconds(5);
                options.PermitLimit = 5; 
                options.QueueLimit = 10; 
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;  
            }).RejectionStatusCode = 429;

            rateLimiterOptions.AddSlidingWindowLimiter("SlidingWindowPolicy", options =>
            {
                options.Window = TimeSpan.FromSeconds(5);  
                options.PermitLimit = 5; 
                options.QueueLimit = 10;  
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;  
                options.SegmentsPerWindow = 5;  
            }).RejectionStatusCode = 429;

            rateLimiterOptions.AddConcurrencyLimiter("ConcurrencyPolicy", options =>
            {
                options.PermitLimit = 1;  
                options.QueueLimit = 2; 
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;  
            }).RejectionStatusCode = 429;

        }); 

        return services;
    }
}