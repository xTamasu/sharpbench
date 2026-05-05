// AppDbContext — EF Core DbContext with all entity DbSets and relationship configuration.

using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<TaskItem> Tasks { get; set; } = null!;
    public DbSet<TaskComment> TaskComments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Task configuration
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(5000);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Priority).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // FK: CreatedBy
            entity.HasOne(e => e.CreatedBy)
                  .WithMany(u => u.CreatedTasks)
                  .HasForeignKey(e => e.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);

            // FK: AssignedTo
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
            entity.Property(e => e.CreatedAt).IsRequired();

            // FK: Task
            entity.HasOne(e => e.Task)
                  .WithMany(t => t.Comments)
                  .HasForeignKey(e => e.TaskId)
                  .OnDelete(DeleteBehavior.Cascade);

            // FK: Author
            entity.HasOne(e => e.Author)
                  .WithMany(u => u.Comments)
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
