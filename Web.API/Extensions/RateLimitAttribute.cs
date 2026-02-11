namespace Web.API.Extensions;

/// <summary>
/// How to partition rate limits.
/// </summary>
public enum Per
{
    /// <summary>
    /// Rate limit per IP address.
    /// </summary>
    Ip,

    /// <summary>
    /// Rate limit per authenticated user (falls back to IP if not authenticated).
    /// </summary>
    User,

    /// <summary>
    /// Rate limit per session (refresh token, falls back to User, then IP).
    /// </summary>
    Session,
}

/// <summary>
/// Attribute to configure rate limiting on endpoints.
/// Uses either a named policy from appsettings or custom values.
/// </summary>
/// <example>
/// // Using a named policy from appsettings
/// [RateLimit("Auth")]
/// [RateLimit("Api", Per.User)]
///
/// // Using custom values
/// [RateLimit(10, 60)]
/// [RateLimit(30, 60, Per.User)]
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RateLimitAttribute : Attribute
{
    /// <summary>
    /// Named policy from appsettings (if using policy-based limiting).
    /// </summary>
    public string? PolicyName { get; }

    /// <summary>
    /// Maximum number of requests allowed in the time window (if using custom values).
    /// </summary>
    public int? PermitLimit { get; }

    /// <summary>
    /// Time window in seconds (if using custom values).
    /// </summary>
    public int? WindowSeconds { get; }

    /// <summary>
    /// How to partition rate limits (per IP, User, or Session).
    /// </summary>
    public Per Per { get; }

    /// <summary>
    /// Creates a rate limit using a named policy from appsettings.
    /// </summary>
    /// <param name="policyName">Policy name defined in RateLimiting:Policies section.</param>
    /// <param name="per">How to partition (default: IP).</param>
    public RateLimitAttribute(string policyName, Per per = Per.Ip)
    {
        PolicyName = policyName;
        Per = per;
    }

    /// <summary>
    /// Creates a rate limit with custom values.
    /// </summary>
    /// <param name="permitLimit">Maximum requests allowed.</param>
    /// <param name="windowSeconds">Time window in seconds.</param>
    /// <param name="per">How to partition (default: IP).</param>
    public RateLimitAttribute(int permitLimit, int windowSeconds, Per per = Per.Ip)
    {
        PermitLimit = permitLimit;
        WindowSeconds = windowSeconds;
        Per = per;
    }
}

/// <summary>
/// Disables rate limiting for an endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class DisableRateLimitAttribute : Attribute;
