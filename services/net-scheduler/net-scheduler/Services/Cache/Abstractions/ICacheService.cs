namespace NetScheduler.Services.Cache.Abstractions;

using System.Threading.Tasks;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, int ttlSeconds = 60);
}