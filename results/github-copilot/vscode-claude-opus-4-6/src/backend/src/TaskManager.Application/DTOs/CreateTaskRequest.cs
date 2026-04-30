// DTO for creating a new task.
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? AssignedToId { get; set; }
}
