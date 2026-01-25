namespace Application.Common.Interfaces;

/// <summary>
/// Cache service interface for dependency inversion.
/// Defined in Application layer, implemented in Infrastructure.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached value or null if not found.</returns>
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in cache with optional expiration.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">Optional expiration time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes a value from cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all values matching a key pattern from cache.
    /// </summary>
    /// <param name="pattern">The key pattern (e.g., "products:*").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
}
