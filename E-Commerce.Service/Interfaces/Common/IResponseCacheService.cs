using StackExchange.Redis;
using System;

namespace E_Commerce.Application.Interfaces.Common
{
    public interface IResponseCacheService
    {
        Task CacheResponseAsync(string cacheKey, object? response, TimeSpan timeToLive);

        Task<string?> GetCachedResponseAsync(string cacheKey);

        }
    }
