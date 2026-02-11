namespace Application.Common.Interfaces;

/// <summary>
/// Service for executing background jobs.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Enqueues a job for background execution.
    /// </summary>
    public Task EnqueueAsync<T>(T job, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Enqueues a named job with payload for background execution.
    /// </summary>
    public Task EnqueueAsync(string jobName, object payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules a job for delayed execution.
    /// </summary>
    public Task ScheduleAsync<T>(T job, TimeSpan delay, CancellationToken cancellationToken = default)
        where T : class;
}
