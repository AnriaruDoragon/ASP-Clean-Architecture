using System.Security.Claims;
using System.Threading.RateLimiting;

namespace Web.API.Extensions;

/// <summary>
/// Rate limiting configuration settings.
/// </summary>
public sealed class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Global rate limit applied to all requests (per IP).
    /// </summary>
    public GlobalLimitSettings Global { get; init; } = new();

    /// <summary>
    /// Named policies that can be referenced by [RateLimit("PolicyName")].
    /// </summary>
    public Dictionary<string, PolicySettings> Policies { get; init; } = [];
}

public sealed class GlobalLimitSettings
{
    public int Limit { get; init; } = 100;
    public int Window { get; init; } = 60;
}

public sealed class PolicySettings
{
    public int Limit { get; init; }
    public int Window { get; init; }
}

/// <summary>
/// Rate limiting configuration extensions.
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Adds rate limiting services with attribute-based configuration.
    /// Policies are defined in appsettings.json under RateLimiting:Policies.
    /// </summary>
    public static IServiceCollection AddRateLimitingPolicies(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        RateLimitingSettings settings =
            configuration.GetSection(RateLimitingSettings.SectionName).Get<RateLimitingSettings>()
            ?? new RateLimitingSettings();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/problem+json";

                double? retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retry)
                    ? retry.TotalSeconds
                    : null;

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        type = "https://tools.ietf.org/html/rfc6585#section-4",
                        title = "Too Many Requests",
                        status = 429,
                        detail = "Rate limit exceeded. Please try again later.",
                        retryAfter,
                    },
                    cancellationToken
                );
            };

            // Global rate limiter - applies to all requests per IP
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                Endpoint? endpoint = context.GetEndpoint();

                // Skip if disabled
                return endpoint?.Metadata.GetMetadata<DisableRateLimitAttribute>() is not null
                    ? RateLimitPartition.GetNoLimiter("disabled")
                    : RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: $"global:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = settings.Global.Limit,
                            Window = TimeSpan.FromSeconds(settings.Global.Window),
                        }
                    );
            });

            // Endpoint policy - reads from [RateLimit] attribute
            options.AddPolicy(
                "endpoint",
                context =>
                {
                    Endpoint? endpoint = context.GetEndpoint();

                    // Skip if disabled
                    if (endpoint?.Metadata.GetMetadata<DisableRateLimitAttribute>() is not null)
                    {
                        return RateLimitPartition.GetNoLimiter("disabled");
                    }

                    // Get rate limit attribute
                    RateLimitAttribute? attr = endpoint?.Metadata.GetMetadata<RateLimitAttribute>();
                    if (attr is null)
                    {
                        return RateLimitPartition.GetNoLimiter("no-limit");
                    }

                    // Resolve limits (from policy name or custom values)
                    int limit;
                    int window;

                    if (!string.IsNullOrEmpty(attr.PolicyName))
                    {
                        if (!settings.Policies.TryGetValue(attr.PolicyName, out var policy))
                        {
                            // Policy not found - use defaults
                            return RateLimitPartition.GetNoLimiter($"unknown-policy:{attr.PolicyName}");
                        }
                        limit = policy.Limit;
                        window = policy.Window;
                    }
                    else
                    {
                        limit = attr.PermitLimit ?? 30;
                        window = attr.WindowSeconds ?? 60;
                    }

                    // Build partition key based on Per type
                    string partitionKey = BuildPartitionKey(context, attr.Per, attr.PolicyName);

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = limit,
                            Window = TimeSpan.FromSeconds(window),
                        }
                    );
                }
            );
        });

        return services;
    }

    private static string BuildPartitionKey(HttpContext context, Per per, string? policyName)
    {
        string identifier = per switch
        {
            Per.User => GetUserIdentifier(context),
            Per.Session => GetSessionIdentifier(context),
            _ => GetIpIdentifier(context),
        };

        // Include policy name to separate counters per policy
        string prefix = policyName ?? context.Request.Path.Value ?? "default";
        return $"{prefix}:{identifier}";
    }

    private static string GetIpIdentifier(HttpContext context) =>
        $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

    private static string GetUserIdentifier(HttpContext context)
    {
        string? userId =
            context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub")
            ?? context.User.FindFirstValue("id");

        return !string.IsNullOrEmpty(userId) ? $"user:{userId}" : GetIpIdentifier(context);
    }

    private static string GetSessionIdentifier(HttpContext context)
    {
        // Try to get session from refresh token or session cookie
        string? sessionId =
            context.Request.Headers["X-Session-Id"].FirstOrDefault() ?? context.Request.Cookies["session_id"];

        return !string.IsNullOrEmpty(sessionId)
            ? $"session:{sessionId}"
            :
            // Fall back to user, then IP
            GetUserIdentifier(context);
    }
}
