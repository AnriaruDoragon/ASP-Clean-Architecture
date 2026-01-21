using System.Security.Cryptography;
using Application.Common.Interfaces;

namespace Infrastructure.Auth;

/// <summary>
/// Password hashing using PBKDF2 with SHA-256.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100000; // OWASP recommended minimum

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        // Format: iterations.salt.hash (all base64)
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hashedPassword)
    {
        string[] parts = hashedPassword.Split('.');
        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[0], out var iterations))
            return false;

        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] hash = Convert.FromBase64String(parts[2]);

        byte[] testHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            hash.Length);

        return CryptographicOperations.FixedTimeEquals(hash, testHash);
    }
}
