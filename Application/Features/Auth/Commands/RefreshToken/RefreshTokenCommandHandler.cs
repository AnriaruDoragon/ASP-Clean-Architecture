using Application.Common.Interfaces;
using Application.Common.Messaging;
using Application.Common.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IApplicationDbContext context,
    IJwtService jwtService,
    IDateTimeProvider dateTimeProvider
) : ICommandHandler<RefreshTokenCommand, AuthTokens>
{
    public async Task<Result<AuthTokens>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Get user ID from expired access token
        Guid? userId = jwtService.GetUserIdFromToken(request.AccessToken);

        if (userId is null)
            return Result.Failure<AuthTokens>(Error.Create(ErrorCode.InvalidToken));

        // Find the refresh token
        Domain.Entities.RefreshToken? refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.Token == request.RefreshToken && rt.UserId == userId,
            cancellationToken
        );

        if (refreshToken is null || !refreshToken.IsValid)
            return Result.Failure<AuthTokens>(Error.Create(ErrorCode.InvalidRefreshToken));

        // Get user
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return Result.Failure<AuthTokens>(Error.Create(ErrorCode.UserNotFound));

        // Revoke old refresh token
        refreshToken.Revoke();

        // Create new refresh token (token rotation for security)
        string newRefreshTokenValue = jwtService.GenerateRefreshToken();
        var newRefreshToken = Domain.Entities.RefreshToken.Create(
            user.Id,
            newRefreshTokenValue,
            dateTimeProvider.UtcNow.AddDays(7),
            refreshToken.DeviceName,
            refreshToken.UserAgent
        );

        context.RefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync(cancellationToken);

        // Generate new access token
        string accessToken = jwtService.GenerateAccessToken(user);

        return new AuthTokens(accessToken, newRefreshTokenValue, dateTimeProvider.UtcNow.AddMinutes(15));
    }
}
