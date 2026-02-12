using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.VerifyEmail;

/// <summary>
/// Command to verify email using a token.
/// </summary>
public sealed record VerifyEmailCommand(Guid UserId, string Token) : IRequest<Result>;

public sealed class VerifyEmailCommandHandler(IApplicationDbContext context)
    : IRequestHandler<VerifyEmailCommand, Result>
{
    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        User? user = await context.Users.FindAsync([request.UserId], cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("User", request.UserId));
        }

        if (user.EmailVerified)
        {
            return Result.Failure(Error.Create(ErrorCode.EmailAlreadyVerified));
        }

        EmailVerificationToken? verificationToken = await context.EmailVerificationTokens.FirstOrDefaultAsync(
            t => t.UserId == user.Id && t.Token == request.Token && !t.IsUsed,
            cancellationToken
        );

        if (verificationToken is null || !verificationToken.IsValid())
        {
            return Result.Failure(Error.Create(ErrorCode.InvalidVerificationToken));
        }

        // Verify email
        user.VerifyEmail();
        verificationToken.MarkAsUsed();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
