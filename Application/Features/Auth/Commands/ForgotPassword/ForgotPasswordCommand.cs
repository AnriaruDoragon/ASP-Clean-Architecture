using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Command to initiate password reset process.
/// </summary>
public sealed record ForgotPasswordCommand(string Email) : IRequest<Result>;
