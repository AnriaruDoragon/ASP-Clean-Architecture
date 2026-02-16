using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.VerifyEmail;

/// <summary>
/// Command to verify email using a token.
/// </summary>
public sealed record VerifyEmailCommand(string Token) : IRequest<Result>;
