using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Command to initiate password reset process.
/// </summary>
public sealed record ForgotPasswordCommand(string Email) : IRequest<Result>;

public sealed class ForgotPasswordCommandHandler(
    IApplicationDbContext context,
    IEmailService emailService) : IRequestHandler<ForgotPasswordCommand, Result>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Always return success to prevent email enumeration
        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (user is null)
        {
            // Don't reveal that user doesn't exist
            return Result.Success();
        }

        // Invalidate any existing tokens for this user
        List<PasswordResetToken> existingTokens = await context.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (PasswordResetToken token in existingTokens)
        {
            token.MarkAsUsed();
        }

        // Create new reset token
        var resetToken = PasswordResetToken.Create(user.Id);
        context.PasswordResetTokens.Add(resetToken);
        await context.SaveChangesAsync(cancellationToken);

        // Send email
        await emailService.SendAsync(new EmailMessage
        {
            To = user.Email,
            Subject = "Password Reset Request",
            Body = $"""
                <h2>Password Reset</h2>
                <p>You requested a password reset. Use the following token to reset your password:</p>
                <p><strong>{resetToken.Token}</strong></p>
                <p>This token will expire in 1 hour.</p>
                <p>If you didn't request this, please ignore this email.</p>
                """
        }, cancellationToken);

        return Result.Success();
    }
}
