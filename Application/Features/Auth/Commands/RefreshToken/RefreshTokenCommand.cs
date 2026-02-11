using Application.Common.Messaging;

namespace Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Command to refresh authentication tokens.
/// </summary>
public sealed record RefreshTokenCommand(string AccessToken, string RefreshToken) : ICommand<AuthTokens>;
