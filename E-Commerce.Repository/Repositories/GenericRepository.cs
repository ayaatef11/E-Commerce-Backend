using E_Commerce.Repository.Repositories.Interfaces;
using E_Commerce.Repository.Specifications;
using E_Commerce.Repository.Specifications.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Repository.Repositories;
public class GenericRepository<T>(StoreContext storeContext) : IGenericRepository<T> where T : BaseEntity
{
    public async Task<IReadOnlyList<T>> GetAllAsync() => await storeContext.Set<T>().AsNoTracking().ToListAsync();

    public async Task<T?> GetByIdAsync(int id) => await storeContext.Set<T>().FindAsync(id);
    public async Task<T?> GetByIdAsync(string id) => await storeContext.Set<T>().FindAsync(id);
    public async Task<List<T>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await storeContext.Set<T>()
            .Where(e => ids.Contains(EF.Property<int>(e, "Id")))
            .ToListAsync();
    }

    public async Task<T?> GetByNameAsync(string name) => await storeContext.Set<T>().FirstOrDefaultAsync(e => EF.Property<string>(e, "Name") == name);

    public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecifications<T> spec)
    {
        return await SpecificationsEvaluator<T>.GetQuery(storeContext.Set<T>(), spec).ToListAsync();
    }
    public async Task<int> GetCountAsync(ISpecifications<T> spec)
    {
        return await SpecificationsEvaluator<T>.GetQuery(storeContext.Set<T>(), spec).CountAsync();
    }
    public async Task<T?> GetByIdWithSpecAsync(ISpecifications<T> spec)
    {
        return await SpecificationsEvaluator<T>.GetQuery(storeContext.Set<T>(), spec).FirstOrDefaultAsync();
    }
    public async Task<T> GetByIdIgnoreFiltersAsync(int id)
    {
        return await storeContext.Set<T>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id);
    }
    public async Task AddAsync(T entity)
    {
        await storeContext.Set<T>().AddAsync(entity);
    }

    public void Update(T entity)
    {
        storeContext.Set<T>().Update(entity);
    }
    public void Delete(T entity)
    {
        storeContext.Set<T>().Remove(entity);
    }
    public async Task<bool> DeleteById(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            Delete(entity);
            return true;
        }
        return false;
    }
    public async Task<T> GetLastOrDefaultAsync()
    {
        return await storeContext.Set<T>().LastOrDefaultAsync();
    }

}


