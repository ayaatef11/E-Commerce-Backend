namespace E_Commerce.Repository.Repositories.Interfaces
{
    public interface IChangeLogRepository<T>:IGenericRepository<T>where T:BaseEntity
    {
        ChangeLog GetByEntityAsync(string entityName,string entityId);
        Task<(IEnumerable<ChangeLog> Logs, int TotalCount)> GetPaginatedAsync(int start, int length, string searchValue, string sortColumn, string sortDirection);
    }
}
