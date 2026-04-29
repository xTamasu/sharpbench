// IPasswordHasher.cs
// Abstraction over password hashing to keep BCrypt out of the Application layer.
namespace TaskManager.Application.Common;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
