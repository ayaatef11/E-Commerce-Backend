using E_Commerce.Core.Data;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Application.Common.BackGroundJobs;
public class DeletedItemsCleanupService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<DeletedItemsCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1);  

        public DeletedItemsCleanupService(
            IServiceProvider services,
            ILogger<DeletedItemsCleanupService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Deleted Items Cleanup Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<StoreContext>();
                        var thresholdDate = DateTime.UtcNow.AddMonths(-3);

                        var itemsToDelete = await dbContext.Products
                            .Where(x => x.IsDeleted && x.DeletedDate <= thresholdDate)
                            .ToListAsync(stoppingToken);

                        if (itemsToDelete.Any())
                        {
                            _logger.LogInformation($"Deleting {itemsToDelete.Count} items permanently");
                            dbContext.Products.RemoveRange(itemsToDelete);
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up deleted items");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
   
}



