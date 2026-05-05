// DTOs for user authentication operations (register and login requests/responses).

namespace TaskManager.Application.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string DisplayName);

public record LoginRequest(
    string Email,
    string Password);

public record AuthResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string Token);

public record UserDto(
    Guid Id,
    string Email,
    string DisplayName);
