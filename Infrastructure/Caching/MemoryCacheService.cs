using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Infrastructure.Caching;

/// <summary>
/// In-memory cache service implementation using IMemoryCache.
/// </summary>
public sealed class MemoryCacheService(IMemoryCache cache, IOptions<CacheSettings> settings) : ICacheService
{
    private readonly CacheSettings _settings = settings.Value;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        T? value = cache.Get<T>(key);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        TimeSpan cacheExpiration = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);

        var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheExpiration };

        options.RegisterPostEvictionCallback(
            (evictedKey, _, _, _) =>
            {
                _keys.TryRemove(evictedKey.ToString()!, out _);
            }
        );

        cache.Set(key, value, options);
        _keys.TryAdd(key, 0);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cache.Remove(key);
        _keys.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var regex = new Regex(
            "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        var keysToRemove = _keys.Keys.Where(k => regex.IsMatch(k)).ToList();

        foreach (string key in keysToRemove)
        {
            cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
