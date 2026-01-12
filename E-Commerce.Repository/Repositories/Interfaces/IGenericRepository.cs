using E_Commerce.Repository.Specifications.Interfaces;

namespace E_Commerce.Repository.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(string id);
        Task<List<T>> GetByIdsAsync(IEnumerable<int> ids);
        Task<T?> GetByNameAsync(string name);
        Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecifications<T> spec);
        Task<int> GetCountAsync(ISpecifications<T> spec);
        Task<T?> GetByIdWithSpecAsync(ISpecifications<T> spec);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<bool> DeleteById(int id);
        Task<T> GetLastOrDefaultAsync();
        Task<T> GetByIdIgnoreFiltersAsync(int id);


    }
}

