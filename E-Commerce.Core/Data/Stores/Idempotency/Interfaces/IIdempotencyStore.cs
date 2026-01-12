using E_Commerce.Core.Shared.Utilties.Identity;

namespace E_Commerce.Core.Data.Stores.Idempotency.Interfaces;
public interface IIdempotencyStore
{
    Task<IdempotencyEntry> GetAsync(string key);
    Task SetAsync(string key, IdempotencyEntry entry);
    Task<bool> AcquireLockAsync(string key);
    Task ReleaseLockAsync(string key);
}

