using Application.Common.Interfaces;
using Application.Common.Messaging;
using Application.Common.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<LoginCommand, AuthTokens>
{
    public async Task<Result<AuthTokens>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (user is null)
            return Result.Failure<AuthTokens>(Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));

        // Verify password
        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<AuthTokens>(Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));

        // Create refresh token
        string refreshTokenValue = jwtService.GenerateRefreshToken();
        var refreshToken = Domain.Entities.RefreshToken.Create(
            user.Id,
            refreshTokenValue,
            dateTimeProvider.UtcNow.AddDays(7),
            request.DeviceName,
            request.UserAgent);

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(cancellationToken);

        // Generate access token
        string accessToken = jwtService.GenerateAccessToken(user);

        return new AuthTokens(
            accessToken,
            refreshTokenValue,
            dateTimeProvider.UtcNow.AddMinutes(15));
    }
}
