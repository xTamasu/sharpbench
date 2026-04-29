// Service interface for task CRUD operations.
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskResponse>> GetAllAsync(TaskItemStatus? status, TaskPriority? priority, Guid? assignedToId);
    Task<TaskResponse> GetByIdAsync(Guid id);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid userId);
    Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request);
    Task DeleteAsync(Guid id, Guid userId);
}
