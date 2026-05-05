// Service interface for task business logic.

using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskResponse>> GetTasksAsync(Domain.Enums.TaskStatus? status, Domain.Enums.Priority? priority, Guid? assignedToId);
    Task<TaskDetailResponse> GetTaskByIdAsync(Guid id);
    Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, Guid createdById);
    Task<TaskResponse> UpdateTaskAsync(Guid id, UpdateTaskRequest request, Guid userId);
    Task DeleteTaskAsync(Guid id, Guid userId);
}
