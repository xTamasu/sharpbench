// EF Core DbContext: configures entity mappings and seeds a demo user.
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
        });

        // TaskItem configuration
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(5000);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Priority).IsRequired();

            entity.HasOne(e => e.CreatedBy)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(e => e.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // TaskComment configuration
        modelBuilder.Entity<TaskComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Body).IsRequired().HasMaxLength(2000);

            entity.HasOne(e => e.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed a demo user (password: "Demo123!")
        var demoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = demoUserId,
            Email = "demo@taskmanager.com",
            // BCrypt hash of "Demo123!"
            PasswordHash = "$2a$11$z0xKrbqETQYtYn56IgREKOu2jnygFuAiB6HAfSN55RnOcEQYaCXnW",
            DisplayName = "Demo User",
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
