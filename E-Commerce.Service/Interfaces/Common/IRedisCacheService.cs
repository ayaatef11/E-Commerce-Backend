namespace E_Commerce.Application.Interfaces.Common;
    public interface IRedisCacheService
    {
    T? GetData<T>(string key);
    void SetData<T>(string key, T Data);
    }

