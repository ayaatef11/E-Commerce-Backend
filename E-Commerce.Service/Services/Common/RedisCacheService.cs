using E_Commerce.Application.Interfaces.Common;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace E_Commerce.Application.Services.Common;
public class RedisCacheService(IDistributedCache _cache) : IRedisCacheService
{
    public T? GetData<T>(string key)
    {
        var data = _cache.GetString(key);
        if (data is null) return default;
        return JsonSerializer.Deserialize<T>(data);
    }

    public void SetData<T>(string key, T Data)
    {
        var options = new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        _cache.SetString(key, JsonSerializer.Serialize(Data), options);
    }
}

