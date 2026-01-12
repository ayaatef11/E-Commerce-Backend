using E_Commerce.Core.Models.TrackingModels;

namespace E_Commerce.Application.Interfaces.Common;
    public interface IChangeLogService
    {
        Task<IReadOnlyList<ChangeLog>> GetAllChangeLogsAsync();
        Task<ChangeLog?> GetChangeLogByIdAsync(int id);
        Task AddChangeLogAsync(ChangeLog changeLog);
        void UpdateChangeLog(ChangeLog changeLog);
        Task<bool> DeleteChangeLogAsync(int id);
        Task<(IEnumerable<ChangeLog> Logs, int TotalCount)> GetPaginatedAsync(
            int start,
            int length,
            string searchValue,
            string sortColumn,
            string sortDirection);
        ChangeLog GetChangeLogByEntityAsync(string entityName, string entityId);

    }

