using Application.Common.Interfaces;
using Application.Common.Messaging;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public async Task<Result<CurrentUserResponse>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid userId = currentUserService.UserId;

        CurrentUserResponse? user = await context
            .Users.Where(u => u.Id == userId)
            .Select(u => new CurrentUserResponse(u.Id, u.Email, u.EmailVerified, u.Roles, u.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return user ?? Result.Failure<CurrentUserResponse>(Error.NotFound("User", userId));
    }
}
