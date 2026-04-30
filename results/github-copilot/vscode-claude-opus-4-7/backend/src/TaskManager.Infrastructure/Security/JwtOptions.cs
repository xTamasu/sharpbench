// JwtOptions.cs
// Strongly-typed binding for the "Jwt" configuration section.
namespace TaskManager.Infrastructure.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "TaskManager";
    public string Audience { get; set; } = "TaskManagerClients";
    public int ExpiryMinutes { get; set; } = 120;
}
