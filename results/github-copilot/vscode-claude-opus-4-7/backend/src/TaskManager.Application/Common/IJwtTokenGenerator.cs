// IJwtTokenGenerator.cs
// Abstraction for issuing JWTs; concrete implementation lives in Infrastructure.
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Common;

public interface IJwtTokenGenerator
{
    /// <summary>Generates a signed JWT access token for the given user.</summary>
    string GenerateToken(User user);
}
