using Application.Common.Interfaces;
using Application.Common.Messaging;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        Guid? userId = currentUserService.UserId;

        if (userId is null)
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "User is not authenticated."));

        if (request.RefreshToken is not null)
        {
            // Revoke specific session
            Domain.Entities.RefreshToken? refreshToken = await context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId, cancellationToken);

            if (refreshToken is not null && !refreshToken.IsRevoked)
            {
                refreshToken.Revoke();
            }
        }
        else
        {
            // Revoke all sessions (logout from all devices)
            List<Domain.Entities.RefreshToken> activeTokens = await context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (Domain.Entities.RefreshToken token in activeTokens)
            {
                token.Revoke();
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
