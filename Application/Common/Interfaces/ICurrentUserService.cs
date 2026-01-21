namespace Application.Common.Interfaces;

/// <summary>
/// Service to access the current authenticated user's information.
/// Implemented in the Web.API layer to extract user context from HTTP requests.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's internal identifier.
    /// Returns null if no user is authenticated.
    /// </summary>
    public Guid? UserId { get; }

    /// <summary>
    /// Gets whether a user is currently authenticated.
    /// </summary>
    public bool IsAuthenticated { get; }
}
