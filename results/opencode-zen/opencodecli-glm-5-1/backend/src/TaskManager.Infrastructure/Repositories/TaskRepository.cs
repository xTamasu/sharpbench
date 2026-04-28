// Task repository with filtering support and eager loading of navigation properties
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : Repository<DomainTask>, ITaskRepository
{
    public TaskRepository(Data.AppDbContext context) : base(context) { }

    public async Task<IEnumerable<DomainTask>> GetFilteredAsync(TaskItemStatus? status, TaskPriority? priority, Guid? assignedToId)
    {
        var query = _dbSet
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (assignedToId.HasValue)
            query = query.Where(t => t.AssignedToId == assignedToId.Value);

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<DomainTask?> GetWithCommentsAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}