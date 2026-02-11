namespace Infrastructure.BackgroundJobs;

/// <summary>
/// Provider options for background job processing.
/// </summary>
public enum BackgroundJobProvider
{
    /// <summary>
    /// Executes jobs synchronously (development/simple scenarios).
    /// </summary>
    Instant,

    /// <summary>
    /// Queues jobs in memory with a background worker.
    /// </summary>
    Memory,

    /// <summary>
    /// Disabled - jobs are dropped.
    /// </summary>
    None,
}

/// <summary>
/// Configuration settings for background jobs.
/// </summary>
public sealed class BackgroundJobSettings
{
    public const string SectionName = "BackgroundJobs";

    /// <summary>
    /// Gets or sets the background job provider.
    /// </summary>
    public BackgroundJobProvider Provider { get; init; } = BackgroundJobProvider.Instant;
}
