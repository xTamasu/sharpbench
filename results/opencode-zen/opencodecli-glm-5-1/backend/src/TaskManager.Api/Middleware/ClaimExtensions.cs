// JWT claim resolver to extract user identity from token claims
using System.Security.Claims;

namespace TaskManager.Api.Middleware;

public static class ClaimExtensions
{
    // Extract user ID from the JWT token claims
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");
        return Guid.Parse(userIdClaim!.Value);
    }
}