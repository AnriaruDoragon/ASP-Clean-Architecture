using Application.Common.Interfaces;
using Application.Common.Messaging;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Queries.GetSessions;

public sealed class GetSessionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    : IQueryHandler<GetSessionsQuery, IReadOnlyList<SessionDto>>
{
    public async Task<Result<IReadOnlyList<SessionDto>>> Handle(
        GetSessionsQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid? userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result.Failure<IReadOnlyList<SessionDto>>(
                Error.Unauthorized("Auth.NotAuthenticated", "User is not authenticated.")
            );
        }

        List<SessionDto> sessions = await context
            .RefreshTokens.Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .Select(rt => new SessionDto(
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
