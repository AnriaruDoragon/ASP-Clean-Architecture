using Domain.Entities;

namespace Application.Common.Interfaces;

/// <summary>
/// Service for JWT token generation and validation.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates an access token for the user.
    /// Token contains only the user ID - authorization is done via database lookup.
    /// </summary>
    public string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    public string GenerateRefreshToken();

    /// <summary>
    /// Gets the user ID from a token (without validating expiration).
    /// Used for refresh token flow.
    /// </summary>
    public Guid? GetUserIdFromToken(string token);
}
