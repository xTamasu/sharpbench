// 20260101000000_InitialCreate.cs
// Initial EF Core migration creating the users, tasks, and task_comments tables.
// Demo user seeding happens at runtime in Program.cs (BCrypt hashes are non-deterministic
// and would change every code-generation, which is poor practice for migrations).
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                password_hash = table.Column<string>(type: "text", nullable: false),
                display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_users", x => x.id));

        migrationBuilder.CreateIndex(
            name: "IX_users_email",
            table: "users",
            column: "email",
            unique: true);

        migrationBuilder.CreateTable(
            name: "tasks",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                priority = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                assigned_to_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_tasks", x => x.id);
                table.ForeignKey(
                    name: "FK_tasks_users_assigned_to_id",
                    column: x => x.assigned_to_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_tasks_users_created_by_id",
                    column: x => x.created_by_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_tasks_assigned_to_id", table: "tasks", column: "assigned_to_id");
        migrationBuilder.CreateIndex(name: "IX_tasks_created_by_id", table: "tasks", column: "created_by_id");

        migrationBuilder.CreateTable(
            name: "task_comments",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                task_id = table.Column<Guid>(type: "uuid", nullable: false),
                author_id = table.Column<Guid>(type: "uuid", nullable: false),
                body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                edited_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_task_comments", x => x.id);
                table.ForeignKey(
                    name: "FK_task_comments_tasks_task_id",
                    column: x => x.task_id,
                    principalTable: "tasks",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_task_comments_users_author_id",
                    column: x => x.author_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_task_comments_task_id", table: "task_comments", column: "task_id");
        migrationBuilder.CreateIndex(name: "IX_task_comments_author_id", table: "task_comments", column: "author_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "task_comments");
        migrationBuilder.DropTable(name: "tasks");
        migrationBuilder.DropTable(name: "users");
    }
}
