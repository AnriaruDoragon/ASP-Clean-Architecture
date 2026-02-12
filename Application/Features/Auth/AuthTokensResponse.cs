namespace Application.Features.Auth;

/// <summary>
/// Response containing authentication tokens.
/// </summary>
public sealed record AuthTokensResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
