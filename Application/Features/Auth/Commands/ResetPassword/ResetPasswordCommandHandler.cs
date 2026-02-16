using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        PasswordResetToken? resetToken = await context.PasswordResetTokens.FirstOrDefaultAsync(
            t => t.Token == request.Token && !t.IsUsed,
            cancellationToken
        );

        if (resetToken is null || !resetToken.IsValid())
            return Result.Failure(Error.From(ErrorCode.InvalidPasswordResetToken));

        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == resetToken.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(Error.From(ErrorCode.InvalidPasswordResetToken));

        // Update password
        string newPasswordHash = passwordHasher.Hash(request.NewPassword);
        user.UpdatePassword(newPasswordHash);

        // Mark token as used
        resetToken.MarkAsUsed();

        // Invalidate all refresh tokens for security
        List<Domain.Entities.RefreshToken> refreshTokens = await context
            .RefreshTokens.Where(t => t.UserId == user.Id && !t.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (Domain.Entities.RefreshToken token in refreshTokens)
            token.Revoke();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
