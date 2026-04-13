using EstateIQ.Interfaces;
using StackExchange.Redis;

namespace EstateIQ.Services;

public class RedisCacheService(IConnectionMultiplexer connectionMultiplexer) : IRedisCacheService
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    public Task SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        return _database.StringSetAsync(key, value, expiry: expiry, when: When.Always);
    }

    public async Task<string?> GetStringAsync(string key)
    {
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }
}
