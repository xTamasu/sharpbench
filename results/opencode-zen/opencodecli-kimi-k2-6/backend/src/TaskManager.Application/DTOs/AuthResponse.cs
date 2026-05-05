namespace TaskManager.Application.DTOs;

/// <summary>
/// DTO for authentication responses containing the JWT token and user info.
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
