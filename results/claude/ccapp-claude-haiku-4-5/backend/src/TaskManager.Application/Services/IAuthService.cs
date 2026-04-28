using TaskManager.Application.Dto.Auth;

namespace TaskManager.Application.Services;

/// <summary>
/// Authentication service interface.
/// </summary>
public interface IAuthService
{
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    string GenerateAccessToken(Guid userId, string email);
}
