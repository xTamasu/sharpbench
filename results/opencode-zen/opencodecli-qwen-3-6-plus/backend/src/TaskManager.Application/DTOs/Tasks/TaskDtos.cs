// Task DTOs — request and response shapes for task endpoints.

using System;
using System.Collections.Generic;
using System.Linq;
using TaskManager.Domain.Enums;
using TaskManager.Application.DTOs.Comments;

namespace TaskManager.Application.DTOs.Tasks;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatusEnum Status { get; set; }
    public TaskPriorityEnum Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? AssignedToId { get; set; }
}

public class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatusEnum Status { get; set; }
    public TaskPriorityEnum Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? AssignedToId { get; set; }
}

public class TaskResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatusEnum Status { get; set; }
    public TaskPriorityEnum Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public Guid CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public IEnumerable<CommentResponse> Comments { get; set; } = Enumerable.Empty<CommentResponse>();
}

public class TaskListFilter
{
    public TaskStatusEnum? Status { get; set; }
    public TaskPriorityEnum? Priority { get; set; }
    public Guid? AssignedToId { get; set; }
}
