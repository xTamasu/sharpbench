namespace TaskManager.Application.DTOs;

/// <summary>
/// DTO for user login requests.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
