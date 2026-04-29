// TaskDtos.cs
// Request/response DTOs for the task endpoints.
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public record CreateTaskRequest(
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    Guid? AssignedToId);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    Guid? AssignedToId);

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    UserDto? AssignedTo,
    UserDto CreatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record TaskDetailDto(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    UserDto? AssignedTo,
    UserDto CreatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<CommentDto> Comments);
