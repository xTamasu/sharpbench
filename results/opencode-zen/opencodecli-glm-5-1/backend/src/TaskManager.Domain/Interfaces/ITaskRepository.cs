// Extended repository interface for Task with filtering support
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Interfaces;

public interface ITaskRepository : IRepository<DomainTask>
{
    Task<IEnumerable<DomainTask>> GetFilteredAsync(TaskItemStatus? status, TaskPriority? priority, Guid? assignedToId);
    Task<DomainTask?> GetWithCommentsAsync(Guid id);
}