// TaskRepository.cs
// EF Core implementation of ITaskRepository, with eager-loading helpers
// for the task-detail and filter-list endpoints.
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Infrastructure.Persistence.Repositories;

public class TaskRepository : Repository<TaskItem>, ITaskRepository
{
    public TaskRepository(AppDbContext db) : base(db) { }

    public Task<TaskItem?> GetByIdWithCommentsAsync(Guid id, CancellationToken ct = default)
        => Set
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<TaskItem>> QueryAsync(
        TaskItemStatus? status,
        TaskPriority? priority,
        Guid? assignedToId,
        CancellationToken ct = default)
    {
        var q = Set.AsNoTracking()
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (status.HasValue) q = q.Where(t => t.Status == status.Value);
        if (priority.HasValue) q = q.Where(t => t.Priority == priority.Value);
        if (assignedToId.HasValue) q = q.Where(t => t.AssignedToId == assignedToId.Value);

        return await q.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);
    }
}
