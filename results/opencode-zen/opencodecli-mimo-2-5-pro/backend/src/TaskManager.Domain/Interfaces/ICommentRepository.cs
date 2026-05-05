// Comment-specific repository interface.

using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

public interface ICommentRepository : IRepository<TaskComment>
{
    Task<IEnumerable<TaskComment>> GetByTaskIdAsync(Guid taskId);
}
