// Service interface for task operations
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskResponse>> GetAllAsync(TaskItemStatus? status, TaskPriority? priority, Guid? assignedToId);
    Task<TaskDetailResponse> GetByIdAsync(Guid id);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid userId);
    Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}