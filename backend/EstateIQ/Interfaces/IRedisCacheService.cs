namespace EstateIQ.Interfaces;

public interface IRedisCacheService
{
    Task SetStringAsync(string key, string value, TimeSpan? expiry = null);

    Task<string?> GetStringAsync(string key);

    Task DeleteAsync(string key);
}
