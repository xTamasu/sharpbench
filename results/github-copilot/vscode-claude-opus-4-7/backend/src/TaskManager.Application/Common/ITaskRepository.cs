// ITaskRepository.cs
// Entity-specific repository extension for TaskItem with eager-loading helpers
// (e.g., for fetching a task with its comments included).
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Common;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<TaskItem?> GetByIdWithCommentsAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<TaskItem>> QueryAsync(
        TaskItemStatus? status,
        TaskPriority? priority,
        Guid? assignedToId,
        CancellationToken ct = default);
}
