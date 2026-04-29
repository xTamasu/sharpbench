// DTO for authentication responses containing the JWT token.
namespace TaskManager.Application.DTOs;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}
