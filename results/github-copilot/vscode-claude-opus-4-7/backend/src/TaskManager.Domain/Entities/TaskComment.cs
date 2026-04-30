// TaskComment.cs
// Domain entity representing a comment on a TaskItem.
namespace TaskManager.Domain.Entities;

public class TaskComment
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }
    public TaskItem? Task { get; set; }

    public Guid AuthorId { get; set; }
    public User? Author { get; set; }

    public string Body { get; set; } = string.Empty;

    // Null until the author edits the comment.
    public DateTime? EditedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
