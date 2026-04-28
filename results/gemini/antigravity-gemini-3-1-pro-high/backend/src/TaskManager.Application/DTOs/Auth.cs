using System;

namespace TaskManager.Application.DTOs
{
    // Auth Data Transfer Objects
    public record RegisterRequest(string Email, string Password, string DisplayName);
    
    public record LoginRequest(string Email, string Password);
    
    public record AuthResponse(string Token, Guid UserId, string Email, string DisplayName);
}
