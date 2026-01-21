using Application.Common.Models;
using Application.Features.Auth;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.Commands.RevokeSession;
using Application.Features.Auth.Queries.GetSessions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.API.Authorization;
using Web.API.Extensions;

namespace Web.API.Controllers.V1;

[ApiController]
[Route("[controller]")]
public class AuthController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Register a new user.
    /// </summary>
    [Public]
    [HttpPost("register")]
    [ProducesResponseType<AuthTokens>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.DeviceName,
            Request.Headers.UserAgent);

        Result<AuthTokens> result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    [Public]
    [HttpPost("login")]
    [ProducesResponseType<AuthTokens>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(
            request.Email,
            request.Password,
            request.DeviceName,
            Request.Headers.UserAgent);

        Result<AuthTokens> result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Refresh access token using refresh token.
    /// </summary>
    [Public]
    [HttpPost("refresh")]
    [ProducesResponseType<AuthTokens>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        Result<AuthTokens> result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Logout (revoke refresh token).
    /// If refreshToken is not provided, revokes all sessions.
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        LogoutCommand? command,
        CancellationToken cancellationToken)
    {
        Result result = await sender.Send(command ?? new LogoutCommand(null), cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Get all active sessions for the current user.
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType<IReadOnlyList<SessionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSessions(
        [FromQuery] string? currentRefreshToken,
        CancellationToken cancellationToken)
    {
        var query = new GetSessionsQuery(currentRefreshToken);
        Result<IReadOnlyList<SessionDto>> result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Revoke a specific session.
    /// </summary>
    [HttpDelete("sessions/{sessionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeSession(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var command = new RevokeSessionCommand(sessionId);
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
public sealed record RegisterRequest(
    string Email,
    string Password,
    string? DeviceName = null);

/// <summary>
/// Request to login.
/// </summary>
/// <remarks>
/// UserAgent is automatically captured from request headers.
/// </remarks>
public sealed record LoginRequest(
    string Email,
    string Password,
    string? DeviceName = null);
