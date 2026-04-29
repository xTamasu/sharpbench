// AuthDtos.cs
// Request/response DTOs for the authentication endpoints.
namespace TaskManager.Application.DTOs;

public record RegisterRequest(string Email, string Password, string DisplayName);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, UserDto User);

public record UserDto(Guid Id, string Email, string DisplayName);
