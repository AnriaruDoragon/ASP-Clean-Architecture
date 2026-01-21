using Application.Common.Messaging;

namespace Application.Features.Auth.Commands.Logout;

/// <summary>
/// Command to logout (revoke refresh token).
/// If RefreshToken is null, revokes all user sessions.
/// </summary>
public sealed record LogoutCommand(string? RefreshToken = null) : ICommand;
