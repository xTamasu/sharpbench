using TaskManager.Application.Dto.Task;

namespace TaskManager.Application.Services;

/// <summary>
/// Task service interface.
/// </summary>
public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllTasksAsync(string? status = null, string? priority = null, string? assignedToId = null);
    Task<TaskDto> GetTaskByIdAsync(Guid id);
    Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, Guid userId);
    Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskRequest request, Guid userId);
    Task DeleteTaskAsync(Guid id, Guid userId);
}
