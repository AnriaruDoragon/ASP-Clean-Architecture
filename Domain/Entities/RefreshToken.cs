using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Refresh token for maintaining user sessions across devices.
/// Each token represents an active session on a device.
/// </summary>
public sealed class RefreshToken : BaseEntity
{
    private RefreshToken() { } // EF Core constructor

    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    // Device identification for multi-device UX
    public string? DeviceName { get; private set; }
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Creates a new refresh token for a user session.
    /// </summary>
    public static RefreshToken Create(
        Guid userId,
        string token,
        DateTime expiresAt,
        string? deviceName = null,
        string? userAgent = null
    )
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            DeviceName = deviceName,
            UserAgent = userAgent,
        };
    }

    /// <summary>
    /// Checks if the token is valid (not expired and not revoked).
    /// </summary>
    public bool IsValid => !IsRevoked && ExpiresAt > DateTime.UtcNow;

    /// <summary>
    /// Checks if the token is expired.
    /// </summary>
    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;

    /// <summary>
    /// Revokes the token (logout).
    /// </summary>
    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }
}
