namespace Application.Common.Interfaces;

/// <summary>
/// Service for password hashing and verification.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password.
    /// </summary>
    public string Hash(string password);

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    public bool Verify(string password, string hash);
}
