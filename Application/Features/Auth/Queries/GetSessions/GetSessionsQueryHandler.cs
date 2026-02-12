using Application.Common.Interfaces;
using Application.Common.Messaging;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Queries.GetSessions;

public sealed class GetSessionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    : IQueryHandler<GetSessionsQuery, IReadOnlyList<Session>>
{
    public async Task<Result<IReadOnlyList<Session>>> Handle(
        GetSessionsQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid? userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result.Failure<IReadOnlyList<Session>>(Error.Create(ErrorCode.NotAuthenticated));
        }

        List<Session> sessions = await context
            .RefreshTokens.Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .Select(rt => new Session(
                rt.Id,
                rt.DeviceName,
                rt.UserAgent,
                rt.CreatedAt,
                rt.ExpiresAt,
                rt.Token == request.CurrentRefreshToken
            ))
            .ToListAsync(cancellationToken);

        return sessions;
    }
}
