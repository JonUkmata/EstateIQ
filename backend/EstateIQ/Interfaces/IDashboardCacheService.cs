namespace EstateIQ.Interfaces;

public interface IDashboardCacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
    Task DeleteAsync(string key);
}
