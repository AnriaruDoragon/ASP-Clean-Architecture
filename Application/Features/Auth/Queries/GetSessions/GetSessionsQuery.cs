using Application.Common.Messaging;

namespace Application.Features.Auth.Queries.GetSessions;

/// <summary>
/// Query to get all active sessions for the current user.
/// </summary>
public sealed record GetSessionsQuery(string? CurrentRefreshToken = null) : IQuery<IReadOnlyList<Session>>;

/// <summary>
/// Represents an active user session.
/// </summary>
public sealed record Session(
    Guid Id,
    string? DeviceName,
    string? UserAgent,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsCurrent
);
