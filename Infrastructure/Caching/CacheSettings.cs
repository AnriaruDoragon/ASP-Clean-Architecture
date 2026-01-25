namespace Infrastructure.Caching;

/// <summary>
/// Configuration settings for caching.
/// </summary>
public sealed class CacheSettings
{
    public const string SectionName = "Caching";

    /// <summary>
    /// Gets or sets whether caching is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the cache provider ("Memory", "Redis", or "None").
    /// </summary>
    public string Provider { get; set; } = "Memory";

    /// <summary>
    /// Gets or sets the Redis configuration.
    /// </summary>
    public RedisSettings Redis { get; set; } = new();

    /// <summary>
    /// Gets or sets the default cache expiration in minutes.
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 5;
}

/// <summary>
/// Redis-specific configuration settings.
/// </summary>
public sealed class RedisSettings
{
    /// <summary>
    /// Gets or sets the Redis connection string.
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";
}
