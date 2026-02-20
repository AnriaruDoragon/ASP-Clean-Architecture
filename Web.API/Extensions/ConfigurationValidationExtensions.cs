namespace Web.API.Extensions;

/// <summary>
/// Configuration validation extensions.
/// </summary>
public static class ConfigurationValidationExtensions
{
    /// <summary>
    /// Validates required configuration on startup.
    /// </summary>
    public static WebApplication ValidateConfiguration(this WebApplication app)
    {
        // Skip validation in Testing environment (integration tests configure their own services)
        if (app.Environment.EnvironmentName == "Testing")
            return app;

        IConfiguration configuration = app.Configuration;
        ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ConfigurationValidation");
        var errors = new List<string>();

        // Validate connection string
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            errors.Add("ConnectionStrings:DefaultConnection is required");

        // Validate JWT settings
        string? jwtSecretKey = configuration["Jwt:SecretKey"];
        if (string.IsNullOrEmpty(jwtSecretKey))
        {
            errors.Add("Jwt:SecretKey is required");
        }
        else if (jwtSecretKey.Length < 32)
        {
            errors.Add("Jwt:SecretKey must be at least 32 characters long");
        }
        else if (jwtSecretKey == "YOUR-SECRET-KEY-MUST-BE-AT-LEAST-32-CHARACTERS-LONG")
        {
            logger.LogWarning(
                "JWT SecretKey is using the default placeholder value. Please set a secure key in production."
            );
        }

        // Validate JWT issuer and audience
        if (string.IsNullOrEmpty(configuration["Jwt:Issuer"]))
            errors.Add("Jwt:Issuer is required");

        if (string.IsNullOrEmpty(configuration["Jwt:Audience"]))
            errors.Add("Jwt:Audience is required");

        // Log warnings for optional but recommended settings
        if (string.IsNullOrEmpty(configuration["Email:Host"]) || configuration.GetValue<bool>("Email:Enabled") == false)
            logger.LogInformation("Email service is not configured. Email functionality will be disabled.");

        // Check for development-only settings in production
        if (!app.Environment.IsDevelopment())
        {
            string[]? corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            if (corsOrigins is null || corsOrigins.Length == 0)
            {
                logger.LogWarning(
                    "CORS AllowedOrigins is not configured. All origins will be allowed in development mode only."
                );
            }

            if (configuration.GetValue<bool>("Telemetry:ConsoleExporter"))
                logger.LogWarning("Telemetry ConsoleExporter is enabled in production. This may impact performance.");
        }

        // Throw if there are critical errors
        if (errors.Count > 0)
        {
            foreach (string error in errors)
                logger.LogError("Configuration error: {Error}", error);

            throw new InvalidOperationException(
                $"Configuration validation failed with {errors.Count} error(s). See logs for details."
            );
        }

        logger.LogInformation("Configuration validation passed");
        return app;
    }
}
