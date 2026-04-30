// Task comment repository interface: extends generic repository with comment-specific queries.
using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

public interface ITaskCommentRepository : IRepository<TaskComment>
{
    Task<IEnumerable<TaskComment>> GetByTaskIdAsync(Guid taskId);
}
