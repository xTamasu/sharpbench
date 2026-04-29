// DbSeeder.cs
// Idempotently inserts a demo user on application startup.
// Kept here (rather than baked into the migration) because BCrypt hashes are
// non-deterministic and would otherwise change each codegen.
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Persistence;

public static class DbSeeder
{
    public const string DemoEmail = "demo@taskmanager.local";
    public const string DemoPassword = "Password123!";

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(u => u.Email == DemoEmail, ct))
            return;

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = DemoEmail,
            DisplayName = "Demo User",
            PasswordHash = hasher.Hash(DemoPassword),
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);
    }
}
