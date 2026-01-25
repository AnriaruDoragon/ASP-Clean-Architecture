using System.Net;
using System.Text.Json;
using Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Caching;

/// <summary>
/// Redis cache service implementation using StackExchange.Redis.
/// </summary>
public sealed class RedisCacheService(IConnectionMultiplexer redis, IOptions<CacheSettings> settings)
    : ICacheService, IDisposable
{
    private readonly IDatabase _database = redis.GetDatabase();
    private readonly CacheSettings _settings = settings.Value;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        RedisValue value = await _database.StringGetAsync(key);

        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<T>((string)value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        TimeSpan cacheExpiration = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);
        string serialized = JsonSerializer.Serialize(value);

        await _database.StringSetAsync(key, serialized, cacheExpiration);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => await _database.KeyDeleteAsync(key);

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        EndPoint[] endpoints = redis.GetEndPoints();
        foreach (EndPoint endpoint in endpoints)
        {
            IServer server = redis.GetServer(endpoint);
            RedisKey[] keys = server.Keys(pattern: pattern).ToArray();

            if (keys.Length > 0)
            {
                await _database.KeyDeleteAsync(keys);
            }
        }
    }

    public void Dispose() => redis.Dispose();
}
