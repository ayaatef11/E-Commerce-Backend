using E_Commerce.Repository.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Repository.Repositories;
public class ChangeLogRepository(StoreContext _context) : GenericRepository<ChangeLog>(_context), IChangeLogRepository<ChangeLog>
    {
        public  ChangeLog GetByEntityAsync(string entityName, string entityId)
        {
            return _context.ChangeLogs.Where(e => e.EntityName == entityName && e.EntityId == entityId).FirstOrDefault();
          
        }
        public async Task<(IEnumerable<ChangeLog> Logs, int TotalCount)> GetPaginatedAsync(int start, int length, string searchValue, string sortColumn, string sortDirection)
        {
            var query = _context.ChangeLogs.Include(c => c.User).AsNoTracking();

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(c =>
                    c.EntityName.Contains(searchValue) ||
                    c.EntityId.Contains(searchValue) ||
                    c.ActionType.Contains(searchValue) ||
                    c.User != null && c.User.Full_Name.Contains(searchValue));
            }

            query = sortColumn?.ToLower() switch
            {
                "entityname" => sortDirection == "asc"
                    ? query.OrderBy(c => c.EntityName)
                    : query.OrderByDescending(c => c.EntityName),
                "changedate" => sortDirection == "asc"
                    ? query.OrderBy(c => c.ChangeDate)
                    : query.OrderByDescending(c => c.ChangeDate),
                _ => query.OrderByDescending(c => c.ChangeDate)  
            };


            var logs = await query
                .Skip(start)
                .Take(length)
                .ToListAsync();

            return (logs, totalCount);
        }

    }

