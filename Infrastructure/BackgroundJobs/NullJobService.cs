using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

/// <summary>
/// No-op background job service that drops all jobs.
/// Used when background jobs are disabled.
/// </summary>
public sealed class NullJobService(ILogger<NullJobService> logger) : IBackgroundJobService
{
    public Task EnqueueAsync<T>(T job, CancellationToken cancellationToken = default) where T : class
    {
        logger.LogDebug("Background jobs disabled - dropping job {JobType}", typeof(T).Name);
        return Task.CompletedTask;
    }

    public Task EnqueueAsync(string jobName, object payload, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Background jobs disabled - dropping job {JobName}", jobName);
        return Task.CompletedTask;
    }

    public Task ScheduleAsync<T>(T job, TimeSpan delay, CancellationToken cancellationToken = default) where T : class
    {
        logger.LogDebug("Background jobs disabled - dropping scheduled job {JobType}", typeof(T).Name);
        return Task.CompletedTask;
    }
}
