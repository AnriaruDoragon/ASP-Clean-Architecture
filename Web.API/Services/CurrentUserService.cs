using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Interfaces;

namespace Web.API.Services;

/// <summary>
/// Service that provides access to the current authenticated user from HTTP context.
/// Extracts user ID from JWT token's "sub" claim.
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId =>
        TryGetUserId()
        ?? throw new InvalidOperationException(
            "No authenticated user. This service must only be used from authenticated endpoints."
        );

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    private Guid? TryGetUserId()
    {
        ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
        if (user is null)
            return null;

        string? claim =
            user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(claim, out Guid id) ? id : null;
    }
}
