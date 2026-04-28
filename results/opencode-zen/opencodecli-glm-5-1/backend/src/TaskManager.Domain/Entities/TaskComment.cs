// Domain entity representing a comment on a task
namespace TaskManager.Domain.Entities;

public class TaskComment
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public DomainTask Task { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public string Body { get; set; } = string.Empty;
    public DateTime? EditedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}