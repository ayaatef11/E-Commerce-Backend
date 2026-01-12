namespace E_Commerce.Core.Data.Stores.Idempotency;

using E_Commerce.Core.Data.Stores.Idempotency.Interfaces;
using E_Commerce.Core.Shared.Utilties.Identity;
using System.Collections.Concurrent;

public class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, IdempotencyEntry> _store = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public Task<IdempotencyEntry> GetAsync(string key)
    {
        _store.TryGetValue(key, out var entry);
        return Task.FromResult(entry)!;
    }

    public Task SetAsync(string key, IdempotencyEntry entry)
    {
        _store[key] = entry;
        return Task.CompletedTask;
    }

    public async Task<bool> AcquireLockAsync(string key)
    {
        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        return await semaphore.WaitAsync(TimeSpan.Zero);
    }

    public Task ReleaseLockAsync(string key)
    {
        if (_locks.TryGetValue(key, out var semaphore))
            semaphore.Release();

        return Task.CompletedTask;
    }
}