// Extended repository interface for TaskComment
using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

public interface ITaskCommentRepository : IRepository<TaskComment>
{
    Task<IEnumerable<TaskComment>> GetByTaskIdAsync(Guid taskId);
}