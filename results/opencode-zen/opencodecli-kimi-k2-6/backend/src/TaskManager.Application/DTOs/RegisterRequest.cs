namespace TaskManager.Application.DTOs;

/// <summary>
/// DTO for user registration requests.
/// </summary>
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
