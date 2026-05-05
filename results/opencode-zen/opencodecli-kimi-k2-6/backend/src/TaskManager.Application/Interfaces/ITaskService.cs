using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

/// <summary>
/// Service interface for task management operations.
/// </summary>
public interface ITaskService
{
    Task<IReadOnlyList<TaskResponse>> GetAllAsync(TaskFilterRequest? filter, CancellationToken cancellationToken = default);
    Task<TaskDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
