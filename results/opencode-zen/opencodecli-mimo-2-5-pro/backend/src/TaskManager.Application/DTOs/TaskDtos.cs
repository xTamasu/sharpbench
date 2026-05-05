// DTOs for task CRUD operations (create, update, response).

namespace TaskManager.Application.DTOs;

public record CreateTaskRequest(
    string Title,
    string? Description,
    Domain.Enums.Priority Priority,
    DateTime? DueDate,
    Guid? AssignedToId);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    Domain.Enums.TaskStatus Status,
    Domain.Enums.Priority Priority,
    DateTime? DueDate,
    Guid? AssignedToId);

public record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    Domain.Enums.TaskStatus Status,
    Domain.Enums.Priority Priority,
    DateTime? DueDate,
    UserDto? AssignedTo,
    UserDto CreatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record TaskDetailResponse(
    Guid Id,
    string Title,
    string? Description,
    Domain.Enums.TaskStatus Status,
    Domain.Enums.Priority Priority,
    DateTime? DueDate,
    UserDto? AssignedTo,
    UserDto CreatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<CommentResponse> Comments);
