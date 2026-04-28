namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents a comment on a task.
/// </summary>
public class TaskComment
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid AuthorId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime? EditedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Task Task { get; set; } = null!;
    public User Author { get; set; } = null!;
}
