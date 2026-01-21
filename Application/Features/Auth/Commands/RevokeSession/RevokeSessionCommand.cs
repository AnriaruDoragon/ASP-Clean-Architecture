using Application.Common.Messaging;

namespace Application.Features.Auth.Commands.RevokeSession;

/// <summary>
/// Command to revoke a specific session by ID.
/// </summary>
public sealed record RevokeSessionCommand(Guid SessionId) : ICommand;
