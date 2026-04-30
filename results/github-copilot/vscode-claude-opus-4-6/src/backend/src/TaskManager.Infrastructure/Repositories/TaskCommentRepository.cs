// Task comment repository: adds task-based comment lookup to the generic repository.
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class TaskCommentRepository : Repository<TaskComment>, ITaskCommentRepository
{
    public TaskCommentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TaskComment>> GetByTaskIdAsync(Guid taskId)
    {
        return await _dbSet
            .Include(c => c.Author)
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }
}
