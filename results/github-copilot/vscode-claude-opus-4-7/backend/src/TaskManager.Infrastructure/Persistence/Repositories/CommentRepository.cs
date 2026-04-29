// CommentRepository.cs
// EF Core implementation of ICommentRepository.
using TaskManager.Application.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Persistence.Repositories;

public class CommentRepository : Repository<TaskComment>, ICommentRepository
{
    public CommentRepository(AppDbContext db) : base(db) { }
}
