using System.Text.Json;
using EstateIQ.Interfaces;

namespace EstateIQ.Services;

public class DashboardCacheService(
    IRedisCacheService redis,
    ILogger<DashboardCacheService> logger) : IDashboardCacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan RedisRetryDelay = TimeSpan.FromMinutes(1);
    private static long _redisDisabledUntilUtcTicks;

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        if (IsRedisTemporarilyDisabled())
        {
            return null;
        }

        try
        {
            var json = await redis.GetStringAsync(key);
            return json is null ? null : JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            DisableRedisTemporarily();
            logger.LogWarning(ex, "Redis cache get failed for key {Key}. Redis cache will be skipped briefly and data will fall back to SQL.", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        if (IsRedisTemporarilyDisabled())
        {
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            await redis.SetStringAsync(key, json, expiry);
        }
        catch (Exception ex)
        {
            DisableRedisTemporarily();
            logger.LogWarning(ex, "Redis cache set failed for key {Key}. Data will not be cached.", key);
        }
    }

    public async Task DeleteAsync(string key)
    {
        if (IsRedisTemporarilyDisabled())
        {
            return;
        }

        try
        {
            await redis.DeleteAsync(key);
        }
        catch (Exception ex)
        {
            DisableRedisTemporarily();
            logger.LogWarning(ex, "Redis cache delete failed for key {Key}.", key);
        }
    }

    private static bool IsRedisTemporarilyDisabled()
    {
        return DateTimeOffset.UtcNow.UtcTicks < Interlocked.Read(ref _redisDisabledUntilUtcTicks);
    }

    private static void DisableRedisTemporarily()
    {
        Interlocked.Exchange(ref _redisDisabledUntilUtcTicks, DateTimeOffset.UtcNow.Add(RedisRetryDelay).UtcTicks);
    }
}
