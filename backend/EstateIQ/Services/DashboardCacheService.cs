using System.Text.Json;
using EstateIQ.Interfaces;

namespace EstateIQ.Services;

public class DashboardCacheService(
    IRedisCacheService redis,
    ILogger<DashboardCacheService> logger) : IDashboardCacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var json = await redis.GetStringAsync(key);
            return json is null ? null : JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache get failed for key {Key}. Falling back to SQL.", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            await redis.SetStringAsync(key, json, expiry);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache set failed for key {Key}. Data will not be cached.", key);
        }
    }

    public async Task DeleteAsync(string key)
    {
        try
        {
            await redis.DeleteAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache delete failed for key {Key}.", key);
        }
    }
}
