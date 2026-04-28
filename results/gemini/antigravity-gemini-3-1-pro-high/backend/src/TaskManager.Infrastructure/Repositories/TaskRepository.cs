using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Services;
using TaskManager.Domain;

namespace TaskManager.Infrastructure.Repositories
{
    public class TaskRepository : GenericRepository<TaskEntity>, IExtendedTaskRepository
    {
        public TaskRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<TaskEntity>> GetTasksWithDetailsAsync(Status? status, Priority? priority, Guid? assignedToId)
        {
            var query = _dbSet.AsQueryable();

            if (status.HasValue) query = query.Where(t => t.Status == status.Value);
            if (priority.HasValue) query = query.Where(t => t.Priority == priority.Value);
            if (assignedToId.HasValue) query = query.Where(t => t.AssignedToId == assignedToId.Value);

            return await query.ToListAsync();
        }

        public async Task<TaskEntity?> GetTaskWithCommentsAsync(Guid id)
        {
            return await _dbSet
                .Include(t => t.Comments)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
