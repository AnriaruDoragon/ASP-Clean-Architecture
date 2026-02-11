using Application.Common.Messaging;

namespace Application.Features.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user.
/// </summary>
public sealed record LoginCommand(string Email, string Password, string? DeviceName = null, string? UserAgent = null)
    : ICommand<AuthTokens>;
