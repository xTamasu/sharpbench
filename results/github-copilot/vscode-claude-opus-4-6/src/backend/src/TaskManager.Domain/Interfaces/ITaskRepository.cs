// Task repository interface: extends generic repository with task-specific queries (filtering).
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Interfaces;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetFilteredAsync(TaskItemStatus? status, TaskPriority? priority, Guid? assignedToId);
    Task<TaskItem?> GetByIdWithCommentsAsync(Guid id);
}
