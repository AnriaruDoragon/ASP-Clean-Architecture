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
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RegisterCommand, AuthTokens>
{
    public async Task<Result<AuthTokens>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // Check if email already exists
        bool emailExists = await context.Users
            .AnyAsync(u => u.Email.Equals(request.Email, StringComparison.InvariantCultureIgnoreCase), cancellationToken);

        if (emailExists)
            return Result.Failure<AuthTokens>(Error.Conflict("User.EmailTaken", "Email is already registered."));

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
            request.UserAgent);

        context.RefreshTokens.Add(refreshToken);

        await context.SaveChangesAsync(cancellationToken);

        // Generate tokens
        string accessToken = jwtService.GenerateAccessToken(user);

        return new AuthTokens(
            accessToken,
            refreshTokenValue,
            dateTimeProvider.UtcNow.AddMinutes(15));
    }
}
