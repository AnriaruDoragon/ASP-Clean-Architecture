using System.Text.RegularExpressions;

namespace Web.API.Extensions;

/// <summary>
/// CORS configuration extensions.
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// Adds CORS services with configuration from appsettings.
    /// Supports wildcards like "*.example.com" in AllowedOrigins.
    /// </summary>
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        string[] allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    // Check if any origins contain wildcards
                    bool hasWildcards = allowedOrigins.Any(o => o.Contains('*'));

                    if (hasWildcards)
                    {
                        // Use custom origin validation for wildcard support
                        var patterns = allowedOrigins.Select(CreateRegexPattern).ToList();

                        policy.SetIsOriginAllowed(origin => patterns.Any(p => p.IsMatch(origin)));
                    }
                    else
                    {
                        policy.WithOrigins(allowedOrigins);
                    }

                    policy.AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                }
                else if (environment.IsDevelopment())
                {
                    // Allow any origin in development when none configured
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            });
        });

        return services;
    }

    private static Regex CreateRegexPattern(string origin)
    {
        // Convert wildcard pattern to regex
        // *.example.com -> ^https?://[^/]+\.example\.com$
        string escaped = Regex.Escape(origin).Replace("\\*", "[^/]+");

        return new Regex($"^{escaped}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}
