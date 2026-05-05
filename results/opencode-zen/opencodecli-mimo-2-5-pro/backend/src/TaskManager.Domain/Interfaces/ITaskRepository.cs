// Task-specific repository interface with filtering capabilities.

using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetFilteredAsync(
        Enums.TaskStatus? status,
        Enums.Priority? priority,
        Guid? assignedToId);
    Task<TaskItem?> GetWithCommentsAsync(Guid id);
}
