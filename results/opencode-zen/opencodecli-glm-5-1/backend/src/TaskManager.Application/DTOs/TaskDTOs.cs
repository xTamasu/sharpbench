// Data transfer objects for task operations
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
    string? Title,
    string? Description,
    TaskItemStatus? Status,
    TaskPriority? Priority,
    DateTime? DueDate,
    Guid? AssignedToId);

public record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    DateTime? DueDate,
    Guid CreatedById,
    string CreatedByName,
    Guid? AssignedToId,
    string? AssignedToName,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record TaskDetailResponse(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    DateTime? DueDate,
    Guid CreatedById,
    string CreatedByName,
    Guid? AssignedToId,
    string? AssignedToName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<CommentResponse> Comments);