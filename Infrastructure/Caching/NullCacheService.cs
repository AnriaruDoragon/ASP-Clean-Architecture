using Application.Common.Interfaces;

namespace Infrastructure.Caching;

/// <summary>
/// No-op cache service implementation for when caching is disabled.
/// Provides zero overhead - all operations are instant no-ops.
/// </summary>
public sealed class NullCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class => Task.FromResult<T?>(null);

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
        where T : class => Task.CompletedTask;

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
