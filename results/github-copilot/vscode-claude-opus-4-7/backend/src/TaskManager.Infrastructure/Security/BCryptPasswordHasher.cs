// BCryptPasswordHasher.cs
// IPasswordHasher implementation backed by BCrypt.Net-Next.
using TaskManager.Application.Common;

namespace TaskManager.Infrastructure.Security;

public class BCryptPasswordHasher : IPasswordHasher
{
    // Work factor of 11 is a reasonable default for an interactive web app in 2026.
    private const int WorkFactor = 11;

    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            // Malformed hash should be treated as "wrong password" rather than crash.
            return false;
        }
    }
}
