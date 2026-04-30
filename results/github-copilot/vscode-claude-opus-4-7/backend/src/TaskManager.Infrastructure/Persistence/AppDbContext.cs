// AppDbContext.cs
// Entity Framework Core DbContext. Configures PostgreSQL-specific column types
// and the relationships between the three aggregate roots.
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskComment> Comments => Set<TaskComment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // ----- User -----
        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Email).HasColumnName("email").HasMaxLength(256).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            e.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(100).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").IsRequired();
        });

        // ----- TaskItem -----
        b.Entity<TaskItem>(e =>
        {
            e.ToTable("tasks");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasColumnName("description").HasMaxLength(5000);
            // Persist enums as strings for human readability in PG.
            e.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
            e.Property(x => x.Priority).HasColumnName("priority").HasConversion<string>().HasMaxLength(16).IsRequired();
            e.Property(x => x.DueDate).HasColumnName("due_date").HasColumnType("timestamp with time zone");
            e.Property(x => x.AssignedToId).HasColumnName("assigned_to_id");
            e.Property(x => x.CreatedById).HasColumnName("created_by_id").IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").IsRequired();
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone").IsRequired();

            e.HasOne(x => x.CreatedBy)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(x => x.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ----- TaskComment -----
        b.Entity<TaskComment>(e =>
        {
            e.ToTable("task_comments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.TaskId).HasColumnName("task_id").IsRequired();
            e.Property(x => x.AuthorId).HasColumnName("author_id").IsRequired();
            e.Property(x => x.Body).HasColumnName("body").HasMaxLength(2000).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").IsRequired();
            e.Property(x => x.EditedAt).HasColumnName("edited_at").HasColumnType("timestamp with time zone");

            e.HasOne(x => x.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(b);
    }
}
