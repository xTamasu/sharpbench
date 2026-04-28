namespace TaskManager.Application.Dto.Auth;

/// <summary>
/// Request to log in a user.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
