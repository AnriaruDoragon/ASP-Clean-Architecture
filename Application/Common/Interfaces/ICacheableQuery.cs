namespace Application.Common.Interfaces;

/// <summary>
/// Marker interface for queries that can be cached.
/// Implement this interface on query records to enable caching via CachingBehavior.
/// </summary>
public interface ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for this query.
    /// </summary>
    public string CacheKey { get; }

    /// <summary>
    /// Gets the optional cache expiration time.
    /// If null, default expiration from configuration is used.
    /// </summary>
    public TimeSpan? CacheExpiration => null;
}
