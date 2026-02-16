using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    : IRequestHandler<VerifyEmailCommand, Result>
{
    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        Guid userId = currentUserService.UserId;

        User? user = await context.Users.FindAsync([userId], cancellationToken);

        if (user is null)
            return Result.Failure(Error.NotFound("User", userId));

        if (user.EmailVerified)
            return Result.Failure(Error.From(ErrorCode.EmailAlreadyVerified));

        EmailVerificationToken? verificationToken = await context.EmailVerificationTokens.FirstOrDefaultAsync(
            t => t.UserId == user.Id && t.Token == request.Token && !t.IsUsed,
            cancellationToken
        );

        if (verificationToken is null || !verificationToken.IsValid())
            return Result.Failure(Error.From(ErrorCode.InvalidEmailVerificationToken));

        // Verify email
        user.VerifyEmail();
        verificationToken.MarkAsUsed();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
