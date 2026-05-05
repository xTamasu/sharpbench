// EF Core model snapshot — reflects the current state of AppDbContext.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaskManager.Infrastructure.Data;

#nullable disable

namespace TaskManager.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.4")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity("TaskManager.Domain.Entities.TaskComment", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid");
            b.Property<Guid>("AuthorId").HasColumnType("uuid");
            b.Property<string>("Body").HasMaxLength(2000).IsRequired().HasColumnType("character varying(2000)");
            b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<DateTime?>("EditedAt").HasColumnType("timestamp with time zone");
            b.Property<Guid>("TaskId").HasColumnType("uuid");

            b.HasKey("Id");
            b.HasIndex("AuthorId");
            b.HasIndex("TaskId");
            b.ToTable("TaskComments");
        });

        modelBuilder.Entity("TaskManager.Domain.Entities.Task", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid");
            b.Property<Guid?>("AssignedToId").HasColumnType("uuid");
            b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<Guid>("CreatedById").HasColumnType("uuid");
            b.Property<string>("Description").HasMaxLength(5000).HasColumnType("character varying(5000)");
            b.Property<DateTime?>("DueDate").HasColumnType("timestamp with time zone");
            b.Property<int>("Priority").HasColumnType("integer");
            b.Property<int>("Status").HasColumnType("integer");
            b.Property<string>("Title").HasMaxLength(200).IsRequired().HasColumnType("character varying(200)");
            b.Property<DateTime>("UpdatedAt").HasColumnType("timestamp with time zone");

            b.HasKey("Id");
            b.HasIndex("AssignedToId");
            b.HasIndex("CreatedById");
            b.ToTable("Tasks");
        });

        modelBuilder.Entity("TaskManager.Domain.Entities.User", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid");
            b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("DisplayName").HasMaxLength(100).IsRequired().HasColumnType("character varying(100)");
            b.Property<string>("Email").HasMaxLength(256).IsRequired().HasColumnType("character varying(256)");
            b.Property<string>("PasswordHash").IsRequired().HasColumnType("text");

            b.HasKey("Id");
            b.HasIndex("Email").IsUnique();
            b.ToTable("Users");
        });

        modelBuilder.Entity("TaskManager.Domain.Entities.TaskComment", b =>
        {
            b.HasOne("TaskManager.Domain.Entities.Task", "Task").WithMany("Comments").HasForeignKey("TaskId").OnDelete(DeleteBehavior.Cascade).IsRequired();
            b.HasOne("TaskManager.Domain.Entities.User", "Author").WithMany("Comments").HasForeignKey("AuthorId").OnDelete(DeleteBehavior.Restrict).IsRequired();
        });

        modelBuilder.Entity("TaskManager.Domain.Entities.Task", b =>
        {
            b.HasOne("TaskManager.Domain.Entities.User", "AssignedTo").WithMany("AssignedTasks").HasForeignKey("AssignedToId").OnDelete(DeleteBehavior.SetNull);
            b.HasOne("TaskManager.Domain.Entities.User", "CreatedBy").WithMany("CreatedTasks").HasForeignKey("CreatedById").OnDelete(DeleteBehavior.Restrict).IsRequired();
        });
    }
}
