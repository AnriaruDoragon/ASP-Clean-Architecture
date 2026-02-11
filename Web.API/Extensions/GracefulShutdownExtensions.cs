namespace Web.API.Extensions;

/// <summary>
/// Graceful shutdown configuration extensions.
/// </summary>
public static class GracefulShutdownExtensions
{
    /// <summary>
    /// Configures graceful shutdown handling.
    /// </summary>
    public static IServiceCollection AddGracefulShutdown(this IServiceCollection services, IConfiguration configuration)
    {
        int shutdownTimeoutSeconds = configuration.GetValue("GracefulShutdown:TimeoutSeconds", 30);

        services.Configure<HostOptions>(options =>
            options.ShutdownTimeout = TimeSpan.FromSeconds(shutdownTimeoutSeconds)
        );

        return services;
    }

    /// <summary>
    /// Configures the application for graceful shutdown with logging.
    /// </summary>
    public static WebApplication UseGracefulShutdown(this WebApplication app)
    {
        IHostApplicationLifetime lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("GracefulShutdown");

        lifetime.ApplicationStarted.Register(() =>
            logger.LogInformation("Application started. Press Ctrl+C to shut down.")
        );

        lifetime.ApplicationStopping.Register(() => logger.LogInformation("Application is shutting down..."));

        lifetime.ApplicationStopped.Register(() => logger.LogInformation("Application has stopped."));

        return app;
    }
}
