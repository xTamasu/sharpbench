namespace TaskManager.Application.Dto.Auth;

/// <summary>
/// Response after successful login.
/// </summary>
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

/// <summary>
/// User data transfer object.
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
