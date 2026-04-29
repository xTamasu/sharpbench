// IAuthService.cs
// Authentication service contract: register a new user and login an existing one.
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
