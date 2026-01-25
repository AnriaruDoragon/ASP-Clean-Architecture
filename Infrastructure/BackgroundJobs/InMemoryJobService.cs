using System.Threading.Channels;
using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

/// <summary>
/// Represents a queued background job.
/// </summary>
public sealed record BackgroundJob(string Name, object Payload, DateTime? ScheduledAt = null);

/// <summary>
/// Background job service that queues jobs in memory for async processing.
/// Jobs are processed by <see cref="BackgroundJobWorker"/>.
/// </summary>
public sealed class InMemoryJobService(ILogger<InMemoryJobService> logger) : IBackgroundJobService
{
    private readonly Channel<BackgroundJob> _channel = Channel.CreateUnbounded<BackgroundJob>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    internal ChannelReader<BackgroundJob> Reader => _channel.Reader;

    public async Task EnqueueAsync<T>(T job, CancellationToken cancellationToken = default) where T : class
    {
        var jobItem = new BackgroundJob(typeof(T).Name, job);
        await _channel.Writer.WriteAsync(jobItem, cancellationToken);
        logger.LogDebug("Enqueued job {JobType}", typeof(T).Name);
    }

    public async Task EnqueueAsync(string jobName, object payload, CancellationToken cancellationToken = default)
    {
        var jobItem = new BackgroundJob(jobName, payload);
        await _channel.Writer.WriteAsync(jobItem, cancellationToken);
        logger.LogDebug("Enqueued named job {JobName}", jobName);
    }

    public async Task ScheduleAsync<T>(T job, TimeSpan delay, CancellationToken cancellationToken = default) where T : class
    {
        DateTime scheduledAt = DateTime.UtcNow.Add(delay);
        var jobItem = new BackgroundJob(typeof(T).Name, job, scheduledAt);
        await _channel.Writer.WriteAsync(jobItem, cancellationToken);
        logger.LogDebug("Scheduled job {JobType} for {ScheduledAt}", typeof(T).Name, scheduledAt);
    }
}
