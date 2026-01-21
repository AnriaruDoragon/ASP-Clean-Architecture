using Application.Common.Interfaces;
using Application.Common.Messaging;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.RevokeSession;

public sealed class RevokeSessionCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : ICommandHandler<RevokeSessionCommand>
{
    public async Task<Result> Handle(
        RevokeSessionCommand request,
        CancellationToken cancellationToken)
    {
        Guid? userId = currentUserService.UserId;

        if (userId is null)
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "User is not authenticated."));

        Domain.Entities.RefreshToken? session = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Id == request.SessionId && rt.UserId == userId, cancellationToken);

        if (session is null)
            return Result.Failure(Error.NotFound("Session", request.SessionId));

        if (!session.IsRevoked)
        {
            session.Revoke();
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
