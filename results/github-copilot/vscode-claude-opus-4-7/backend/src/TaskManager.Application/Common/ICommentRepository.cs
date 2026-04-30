// ICommentRepository.cs
// Entity-specific repository for TaskComment.
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Common;

public interface ICommentRepository : IRepository<TaskComment>
{
}
