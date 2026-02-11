using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

/// <summary>
/// Background job service that fires-and-forgets jobs immediately after the response.
/// Jobs are dispatched via MediatR in a separate task, not blocking the HTTP response.
/// Suitable for development and simple scenarios.
/// </summary>
public sealed class InstantJobService(IServiceScopeFactory scopeFactory, ILogger<InstantJobService> logger)
    : IBackgroundJobService
{
    public Task EnqueueAsync<T>(T job, CancellationToken cancellationToken = default)
        where T : class
    {
        logger.LogDebug("Fire-and-forget job {JobType}", typeof(T).Name);

        // Fire-and-forget: dispatch in background task, don't await
        _ = DispatchAsync(job, cancellationToken);

        return Task.CompletedTask;
    }

    public Task EnqueueAsync(string jobName, object payload, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Fire-and-forget named job {JobName}", jobName);

        _ = DispatchAsync(payload, cancellationToken);

        return Task.CompletedTask;
    }

    public Task ScheduleAsync<T>(T job, TimeSpan delay, CancellationToken cancellationToken = default)
        where T : class
    {
        logger.LogDebug("Scheduling job {JobType} with delay {Delay}", typeof(T).Name, delay);

        // Fire-and-forget with delay
        _ = DispatchWithDelayAsync(job, delay, cancellationToken);

        return Task.CompletedTask;
    }

    private async Task DispatchAsync(object payload, CancellationToken cancellationToken)
    {
        try
        {
            if (payload is IBaseRequest request)
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(request, cancellationToken);
                logger.LogDebug("Completed job {JobType}", payload.GetType().Name);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing job {JobType}", payload.GetType().Name);
        }
    }

    private async Task DispatchWithDelayAsync(object payload, TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken);
            await DispatchAsync(payload, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation requested
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing scheduled job {JobType}", payload.GetType().Name);
        }
    }
}
