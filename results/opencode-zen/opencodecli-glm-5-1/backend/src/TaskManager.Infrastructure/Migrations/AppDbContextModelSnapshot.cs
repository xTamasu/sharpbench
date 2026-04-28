// Model snapshot for EF Core migration
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TaskManager.Infrastructure.Data;

#nullable disable

namespace TaskManager.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.2")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("TaskManager.Domain.Entities.DomainTask", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<Guid?>("AssignedToId")
                .HasColumnType("uuid");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("CreatedById")
                .HasColumnType("uuid");

            b.Property<string>("Description")
                .HasMaxLength(5000)
                .HasColumnType("character varying(5000)");

            b.Property<DateTime?>("DueDate")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Priority")
                .HasMaxLength(10)
                .HasColumnType("character varying(10)");

            b.Property<string>("Status")
                .HasMaxLength(20)
                .HasColumnType("character varying(20)");

            b.Property<string>("Title")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            b.Property<DateTime>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("AssignedToId");

            b.HasIndex("CreatedById");

            b.ToTable("Tasks");
        });

        modelBuilder.Entity("TaskManager.Domain.Entities.TaskComment", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<Guid>("AuthorId")
                .HasColumnType("uuid");

            b.Property<string>("Body")
                .IsRequired()
                .HasMaxLength(2000)
                .HasColumnType("character varying(2000)");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTime?>("EditedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("TaskId")
                .HasColumnType("uuid");

            b.HasKey("Id");

            b.HasIndex("AuthorId");

            b.HasIndex("TaskId");

            b.ToTable("TaskComments");
        });

        modelBuilder.Entity("TaskManager.Domain.Entities.User", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("DisplayName")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.Property<string>("Email")
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnType("character varying(256)");

            b.Property<string>("PasswordHash")
                .IsRequired()
                .HasColumnType("text");

            b.HasKey("Id");

            b.HasIndex("Email")
                .IsUnique();

            b.ToTable("Users");
        });

        modelBuilder.Entity("TaskManager.Domain.Entities.DomainTask", b =>
        {
            b.HasOne("TaskManager.Domain.Entities.User", "AssignedTo")
                .WithMany("AssignedTasks")
                .HasForeignKey("AssignedToId")
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne("TaskManager.Domain.Entities.User", "CreatedBy")
                .WithMany("CreatedTasks")
                .HasForeignKey("CreatedById")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.Navigation("AssignedTo");

            b.Navigation("CreatedBy");
        });

        modelBuilder.Entity("TaskManager.Domain.Entities.TaskComment", b =>
        {
            b.HasOne("TaskManager.Domain.Entities.DomainTask", "Task")
                .WithMany("Comments")
                .HasForeignKey("TaskId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne("TaskManager.Domain.Entities.User", "Author")
                .WithMany("Comments")
                .HasForeignKey("AuthorId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.Navigation("Author");

            b.Navigation("Task");
        });
#pragma warning restore 612, 618
    }
}