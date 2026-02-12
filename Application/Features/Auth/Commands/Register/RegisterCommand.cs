using Application.Common.Messaging;

namespace Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new user.
/// </summary>
public sealed record RegisterCommand(string Email, string Password, string? DeviceName = null, string? UserAgent = null)
    : ICommand<AuthTokensResponse>;
