using Microsoft.EntityFrameworkCore;
using TaskManager.Domain;

namespace TaskManager.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<TaskEntity> Tasks { get; set; } = null!;
        public DbSet<TaskComment> Comments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<TaskEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(5000);
                
                entity.HasOne(e => e.AssignedTo)
                      .WithMany()
                      .HasForeignKey(e => e.AssignedToId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.CreatedBy)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TaskComment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Body).IsRequired().HasMaxLength(2000);

                entity.HasOne(e => e.Task)
                      .WithMany(t => t.Comments)
                      .HasForeignKey(e => e.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Author)
                      .WithMany()
                      .HasForeignKey(e => e.AuthorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed demo user
            var demoUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = demoUserId,
                Email = "demo@example.com",
                DisplayName = "Demo User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = new System.DateTime(2024, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)
            });
        }
    }
}
