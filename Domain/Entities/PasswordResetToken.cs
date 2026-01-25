using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Password reset token entity for secure password recovery.
/// </summary>
public sealed class PasswordResetToken : BaseEntity
{
    private PasswordResetToken() { } // EF Core constructor

    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Creates a new password reset token.
    /// </summary>
    public static PasswordResetToken Create(Guid userId, TimeSpan? validFor = null)
    {
        return new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.Add(validFor ?? TimeSpan.FromHours(1)),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Checks if the token is valid (not expired and not used).
    /// </summary>
    public bool IsValid() => !IsUsed && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// Marks the token as used.
    /// </summary>
    public void MarkAsUsed() => IsUsed = true;

    private static string GenerateSecureToken()
    {
        byte[] tokenBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
