// DTO for task responses returned by the API.
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public class TaskResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CommentResponse> Comments { get; set; } = new();
}
