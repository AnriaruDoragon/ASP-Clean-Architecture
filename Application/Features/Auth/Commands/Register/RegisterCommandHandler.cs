using Application.Common.Interfaces;
using Application.Common.Messaging;
using Application.Common.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    IDateTimeProvider dateTimeProvider
) : ICommandHandler<RegisterCommand, AuthTokensResponse>
{
    public async Task<Result<AuthTokensResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        string normalizedEmail = request.Email.ToLowerInvariant();
        bool emailExists = await context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailExists)
            return Result.Failure<AuthTokensResponse>(Error.Create(ErrorCode.EmailTaken));

        // Create user
        string passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, passwordHash);

        context.Users.Add(user);

        // Create refresh token
        string refreshTokenValue = jwtService.GenerateRefreshToken();
        var refreshToken = Domain.Entities.RefreshToken.Create(
            user.Id,
            refreshTokenValue,
            dateTimeProvider.UtcNow.AddDays(7),
            request.DeviceName,
            request.UserAgent
        );

        context.RefreshTokens.Add(refreshToken);

        await context.SaveChangesAsync(cancellationToken);

        // Generate tokens
        string accessToken = jwtService.GenerateAccessToken(user);

        return new AuthTokensResponse(accessToken, refreshTokenValue, dateTimeProvider.UtcNow.AddMinutes(15));
    }
}
