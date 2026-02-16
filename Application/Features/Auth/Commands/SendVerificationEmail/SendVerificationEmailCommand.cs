using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.SendVerificationEmail;

/// <summary>
/// Command to send email verification link.
/// </summary>
public sealed record SendVerificationEmailCommand : IRequest<Result>;
