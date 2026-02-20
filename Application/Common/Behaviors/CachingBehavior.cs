using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior for caching query results.
/// Only activates for requests implementing ICacheableQuery.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type (must be a reference type).</typeparam>
public sealed class CachingBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<CachingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        // Only cache if the request implements ICacheableQuery
        if (request is not ICacheableQuery cacheableQuery)
        {
            return await next(cancellationToken);
        }

        string cacheKey = cacheableQuery.CacheKey;

        // Try to get from cache
        TResponse? cachedResponse = await cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResponse is not null)
        {
            logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cachedResponse;
        }

        logger.LogDebug("Cache miss for {CacheKey}", cacheKey);

        // Execute the handler
        TResponse response = await next();

        // Cache the response
        await cacheService.SetAsync(cacheKey, response, cacheableQuery.CacheExpiration, cancellationToken);

        return response;
    }
}
