using E_Commerce.Application.Interfaces.Common;
using E_Commerce.Repository.Repositories;
using E_Commerce.Repository.Repositories.Interfaces;

namespace E_Commerce.Application.Services.Common
{
	public class ChangeLogService(IUnitOfWork _unitOfWork, ChangeLogRepository changeLog_repository, IMemoryCache _cache,
	  ILogger<ChangeLogService> _logger) : IChangeLogService
	{
		private const string CacheKeyPrefix = "ChangeLogs_";

		public async Task AddChangeLogAsync(ChangeLog changeLog)
		{
			if (changeLog.EntityName == nameof(Product))
			{
				var Product_info = await _unitOfWork.Repository<Product>().GetByIdAsync(changeLog.EntityId);
				if (Product_info != null) changeLog.NewValues += $"Product {Product_info.Name},";
			}
			await _unitOfWork.Repository<ChangeLog>().AddAsync(changeLog);
		}

		public Task<bool> DeleteChangeLogAsync(int id)
		{
			return _unitOfWork.Repository<ChangeLog>().DeleteById(id);
		}

		public Task<IReadOnlyList<ChangeLog>> GetAllChangeLogsAsync()
		{
			return _unitOfWork.Repository<ChangeLog>().GetAllAsync();
		}

		public ChangeLog GetChangeLogByEntityAsync(string entityName, string entityId)
		{
			return changeLog_repository.GetByEntityAsync(entityName, entityId);
		}

		public Task<ChangeLog?> GetChangeLogByIdAsync(int id)
		{
			return _unitOfWork.Repository<ChangeLog>().GetByIdAsync(id);
		}

		public void UpdateChangeLog(ChangeLog changeLog)
		{
			_unitOfWork.Repository<ChangeLog>().Update(changeLog);
		}



		public async Task<(IEnumerable<ChangeLog> Logs, int TotalCount)> GetPaginatedAsync(
	   int start,
	   int length,
	   string searchValue,
	   string sortColumn,
	   string sortDirection)
		{
			try
			{
				string cacheKey = $"{CacheKeyPrefix}{start}_{length}_{searchValue}_{sortColumn}_{sortDirection}";

				return await _cache.GetOrCreateAsync(cacheKey, async entry =>
				{
					entry.Size = 1;
					entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
					return await changeLog_repository.GetPaginatedAsync(start, length, searchValue, sortColumn, sortDirection);
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting paginated change logs");
				throw;
			}
		}


	}
}

