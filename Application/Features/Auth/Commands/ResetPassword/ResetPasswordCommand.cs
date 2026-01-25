using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Command to reset password using a valid token.
/// </summary>
public sealed record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword) : IRequest<Result>;

public sealed class ResetPasswordCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher) : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.Validation("Invalid email or token."));
        }

        PasswordResetToken? resetToken = await context.PasswordResetTokens
            .FirstOrDefaultAsync(t =>
                t.UserId == user.Id &&
                t.Token == request.Token &&
                !t.IsUsed,
                cancellationToken);

        if (resetToken is null || !resetToken.IsValid())
        {
            return Result.Failure(Error.Validation("Invalid or expired token."));
        }

        // Update password
        string newPasswordHash = passwordHasher.Hash(request.NewPassword);
        user.UpdatePassword(newPasswordHash);

        // Mark token as used
        resetToken.MarkAsUsed();

        // Invalidate all refresh tokens for security
        List<Domain.Entities.RefreshToken> refreshTokens = await context.RefreshTokens
            .Where(t => t.UserId == user.Id && !t.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (Domain.Entities.RefreshToken token in refreshTokens)
        {
            token.Revoke();
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
