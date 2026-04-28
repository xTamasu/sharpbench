// JWT generation utility interface
namespace TaskManager.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string email, string displayName);
}