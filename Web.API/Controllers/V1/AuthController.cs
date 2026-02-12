using Application.Common.Models;
using Application.Features.Auth;
using Application.Features.Auth.Commands.ForgotPassword;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.Commands.ResetPassword;
using Application.Features.Auth.Commands.RevokeSession;
using Application.Features.Auth.Commands.SendVerificationEmail;
using Application.Features.Auth.Commands.VerifyEmail;
using Application.Features.Auth.Queries.GetSessions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Web.API.Authorization;
using Web.API.Extensions;
using Web.API.Models;

namespace Web.API.Controllers.V1;

[ApiController]
[Route("[controller]")]
[EnableRateLimiting("endpoint")]
public class AuthController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Register a new user.
    /// </summary>
    [Public]
    [HttpPost("[action]")]
    [EndpointSummary("Register")]
    [RateLimit("Auth")]
    [ProducesResponseType<AuthTokensResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.DeviceName,
            Request.Headers.UserAgent
        );

        Result<AuthTokensResponse> result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    [Public]
    [HttpPost("[action]")]
    [EndpointSummary("Login")]
    [RateLimit("Auth")]
    [ProducesResponseType<AuthTokensResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password, request.DeviceName, Request.Headers.UserAgent);

        Result<AuthTokensResponse> result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Refresh access token using refresh token.
    /// </summary>
    [Public]
    [HttpPost("[action]")]
    [EndpointSummary("Refresh token")]
    [ProducesResponseType<AuthTokensResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.AccessToken, request.RefreshToken);
        Result<AuthTokensResponse> result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Logout (revoke refresh token).
    /// If refreshToken is not provided, revokes all sessions.
    /// </summary>
    [HttpPost("[action]")]
    [EndpointSummary("Logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(LogoutRequest? request, CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(request?.RefreshToken);
        Result result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Get all active sessions for the current user.
    /// </summary>
    [HttpGet("Sessions")]
    [EndpointSummary("List sessions")]
    [ProducesResponseType<IReadOnlyList<Session>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSessions(
        [FromQuery] GetSessionsQuery query,
        CancellationToken cancellationToken
    )
    {
        Result<IReadOnlyList<Session>> result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Revoke a specific session.
    /// </summary>
    [HttpDelete("Sessions/{sessionId:guid}")]
    [EndpointSummary("Revoke session")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var command = new RevokeSessionCommand(sessionId);
        Result result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Initiates password reset process.
    /// </summary>
    [HttpPost("[action]")]
    [EndpointSummary("Forgot password")]
    [Public]
    [RateLimit("Auth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand(request.Email);
        Result result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Resets password using a valid token.
    /// </summary>
    [HttpPost("[action]")]
    [EndpointSummary("Reset password")]
    [Public]
    [RateLimit("Auth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(request.Email, request.Token, request.NewPassword);
        Result result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Sends email verification link to the current user.
    /// </summary>
    [HttpPost("[action]")]
    [EndpointSummary("Send verification email")]
    [Authorize]
    [RateLimit("Strict", Per.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendVerificationEmail(CancellationToken cancellationToken)
    {
        string? userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
        {
            return Unauthorized();
        }

        var command = new SendVerificationEmailCommand(userGuid);
        Result result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Verifies email using a token.
    /// </summary>
    [HttpPost("[action]")]
    [EndpointSummary("Verify email")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
        {
            return Unauthorized();
        }

        var command = new VerifyEmailCommand(userGuid, request.Token);
        Result result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }
}

/// <summary>
/// Request to register a new user.
/// </summary>
/// <remarks>
/// UserAgent is automatically captured from request headers.
/// </remarks>
public sealed record RegisterRequest(string Email, string Password, string? DeviceName = null);

/// <summary>
/// Request to login.
/// </summary>
/// <remarks>
/// UserAgent is automatically captured from request headers.
/// </remarks>
public sealed record LoginRequest(string Email, string Password, string? DeviceName = null);

/// <summary>
/// Request to initiate password reset.
/// </summary>
public sealed record ForgotPasswordRequest(string Email);

/// <summary>
/// Request to reset password with token.
/// </summary>
public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);

/// <summary>
/// Request to verify email with token.
/// </summary>
public sealed record VerifyEmailRequest(string Token);

/// <summary>
/// Request to refresh authentication tokens.
/// </summary>
public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken);

/// <summary>
/// Request to logout.
/// </summary>
public sealed record LogoutRequest(string? RefreshToken = null);
