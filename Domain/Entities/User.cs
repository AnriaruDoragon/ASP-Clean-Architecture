using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// User entity for authentication and authorization.
/// </summary>
public sealed class User : AuditableEntity, IAggregateRoot
{
    private readonly List<Role> _roles = [];

    private User() { } // EF Core constructor

    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool EmailVerified { get; private set; }

    /// <summary>
    /// User's roles for authorization.
    /// </summary>
    public IReadOnlyList<Role> Roles => _roles.AsReadOnly();

    /// <summary>
    /// Creates a new user.
    /// </summary>
    public static User Create(string email, string passwordHash, Role initialRole = Role.User)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            EmailVerified = false
        };

        user._roles.Add(initialRole);

        return user;
    }

    /// <summary>
    /// Marks the email as verified.
    /// </summary>
    public void VerifyEmail() => EmailVerified = true;

    /// <summary>
    /// Updates the password hash.
    /// </summary>
    public void UpdatePassword(string passwordHash) => PasswordHash = passwordHash;

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    public bool HasRole(Role role) => _roles.Contains(role);

    /// <summary>
    /// Checks if the user has any of the specified roles.
    /// </summary>
    public bool HasAnyRole(params Role[] roles) => roles.Any(r => _roles.Contains(r));

    /// <summary>
    /// Adds a role to the user.
    /// </summary>
    public void AddRole(Role role)
    {
        if (!_roles.Contains(role))
            _roles.Add(role);
    }

    /// <summary>
    /// Removes a role from the user.
    /// </summary>
    public void RemoveRole(Role role) => _roles.Remove(role);
}
