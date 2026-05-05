// TaskItem entity — represents a manageable task with status, priority, and assignment.

using System;
using System.Collections.Generic;
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatusEnum Status { get; set; }
    public TaskPriorityEnum Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? AssignedToId { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public User? AssignedTo { get; set; }
    public User CreatedBy { get; set; } = null!;
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}
