// CurrentUserAccessor.cs
// Extracts the current authenticated user's id from the JWT "sub" claim.
// Centralized so controllers don't repeat claim parsing logic.
using System.Security.Claims;
using TaskManager.Application.Common;

namespace TaskManager.Api.Auth;

public interface ICurrentUser
{
    Guid Id { get; }
}

public class CurrentUserAccessor : ICurrentUser
{
    public Guid Id { get; }

    public CurrentUserAccessor(IHttpContextAccessor http)
    {
        var claim = http.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
                    ?? http.HttpContext?.User?.FindFirst("sub");
        if (claim is null || !Guid.TryParse(claim.Value, out var id))
            throw new AuthenticationFailedException("Authenticated user id is missing or invalid.");
        Id = id;
    }
}
