using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Command to reset password using a valid token.
/// </summary>
public sealed record ResetPasswordCommand(string Token, string NewPassword) : IRequest<Result>;
