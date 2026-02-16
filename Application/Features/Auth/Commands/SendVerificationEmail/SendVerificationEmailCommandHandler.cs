using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.SendVerificationEmail;

public sealed class SendVerificationEmailCommandHandler(
    IApplicationDbContext context,
    IEmailService emailService,
    ICurrentUserService currentUserService
) : IRequestHandler<SendVerificationEmailCommand, Result>
{
    public async Task<Result> Handle(SendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        Guid userId = currentUserService.UserId;

        User? user = await context.Users.FindAsync([userId], cancellationToken);

        if (user is null)
            return Result.Failure(Error.NotFound("User", userId));

        if (user.EmailVerified)
            return Result.Failure(Error.From(ErrorCode.EmailAlreadyVerified, "email"));

        // Invalidate existing tokens
        List<EmailVerificationToken> existingTokens = await context
            .EmailVerificationTokens.Where(t => t.UserId == user.Id && !t.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (EmailVerificationToken token in existingTokens)
            token.MarkAsUsed();

        // Create new verification token
        var verificationToken = EmailVerificationToken.Create(user.Id);
        context.EmailVerificationTokens.Add(verificationToken);
        await context.SaveChangesAsync(cancellationToken);

        // Send email
        await emailService.SendAsync(
            new EmailMessage
            {
                To = user.Email,
                Subject = "Verify Your Email Address",
                Body = $"""
                <h2>Email Verification</h2>
                <p>Please verify your email address using the following token:</p>
                <p><strong>{verificationToken.Token}</strong></p>
                <p>This token will expire in 24 hours.</p>
                <p>If you didn't create an account, please ignore this email.</p>
                """,
            },
            cancellationToken
        );

        return Result.Success();
    }
}
