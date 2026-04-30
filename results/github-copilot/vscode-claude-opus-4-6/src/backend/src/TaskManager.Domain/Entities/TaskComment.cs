// TaskComment entity: represents a comment on a task.
namespace TaskManager.Domain.Entities;

public class TaskComment
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid AuthorId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime? EditedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public TaskItem Task { get; set; } = null!;
    public User Author { get; set; } = null!;
}
