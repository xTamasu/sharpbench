// TaskItem.cs
// Domain entity representing a task tracked by the system.
// Named TaskItem (not "Task") to avoid clashing with System.Threading.Tasks.Task.
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }

    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }

    public Guid CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}
