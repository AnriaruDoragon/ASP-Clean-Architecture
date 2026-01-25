using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

/// <summary>
/// Background worker that processes jobs from the in-memory queue.
/// Jobs implementing IRequest are dispatched via MediatR.
/// </summary>
public sealed class BackgroundJobWorker(
    InMemoryJobService jobService,
    IServiceScopeFactory scopeFactory,
    ILogger<BackgroundJobWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Background job worker started");

        await foreach (BackgroundJob job in jobService.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                // Handle scheduled jobs
                if (job.ScheduledAt.HasValue)
                {
                    TimeSpan delay = job.ScheduledAt.Value - DateTime.UtcNow;
                    if (delay > TimeSpan.Zero)
                    {
                        logger.LogDebug("Waiting {Delay} for scheduled job {JobName}", delay, job.Name);
                        await Task.Delay(delay, cancellationToken);
                    }
                }

                logger.LogDebug("Processing job {JobName}", job.Name);

                // Dispatch via MediatR if payload implements IRequest
                if (job.Payload is IBaseRequest request)
                {
                    using IServiceScope scope = scopeFactory.CreateScope();
                    IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    await mediator.Send(request, cancellationToken);
                }

                logger.LogDebug("Completed job {JobName}", job.Name);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing job {JobName}", job.Name);
            }
        }

        logger.LogInformation("Background job worker stopped");
    }
}
