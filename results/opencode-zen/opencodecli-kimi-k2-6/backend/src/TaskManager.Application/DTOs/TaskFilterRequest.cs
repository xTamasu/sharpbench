using TaskManager.Domain.Enums;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.DTOs;

/// <summary>
/// DTO for filtering tasks in list queries.
/// </summary>
public class TaskFilterRequest
{
    public TaskStatus? Status { get; set; }
    public Priority? Priority { get; set; }
    public Guid? AssignedToId { get; set; }
}
