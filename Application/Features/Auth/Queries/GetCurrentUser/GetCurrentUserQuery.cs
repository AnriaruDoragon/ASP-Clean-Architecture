using Application.Common.Messaging;
using Domain.Enums;

namespace Application.Features.Auth.Queries.GetCurrentUser;

/// <summary>
/// Query to get the current authenticated user's profile.
/// </summary>
public sealed record GetCurrentUserQuery : IQuery<CurrentUserResponse>;

/// <summary>
/// Represents the current user's profile.
/// </summary>
public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    bool EmailVerified,
    IReadOnlyList<Role> Roles,
    DateTime CreatedAt
);
