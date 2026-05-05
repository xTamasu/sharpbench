// Database seeder that creates a demo user on initial migration.

using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        // Seed a demo user if none exist
        if (!context.Users.Any())
        {
            context.Users.Add(new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Email = "demo@taskmanager.com",
                // BCrypt hash of "Demo123!"
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
                DisplayName = "Demo User",
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();
        }
    }
}
