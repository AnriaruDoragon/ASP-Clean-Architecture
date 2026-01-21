namespace Infrastructure.Auth;

/// <summary>
/// JWT configuration settings.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// Secret key for signing tokens. Must be at least 32 characters.
    /// </summary>
    public string SecretKey { get; init; } = null!;

    /// <summary>
    /// Token issuer (your API).
    /// </summary>
    public string Issuer { get; init; } = null!;

    /// <summary>
    /// Token audience (your clients).
    /// </summary>
    public string Audience { get; init; } = null!;

    /// <summary>
    /// Access token expiration in minutes.
    /// </summary>
    public int AccessTokenExpirationMinutes { get; init; } = 15;

    /// <summary>
    /// Refresh token expiration in days.
    /// </summary>
    public int RefreshTokenExpirationDays { get; init; } = 7;
}
