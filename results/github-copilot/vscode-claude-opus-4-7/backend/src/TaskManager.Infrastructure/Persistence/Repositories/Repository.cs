// Repository.cs
// Generic EF Core repository implementation. Concrete entity-specific repositories
// inherit and add helpers (eager loading, query shortcuts).
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common;

namespace TaskManager.Infrastructure.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Db;
    protected readonly DbSet<T> Set;

    public Repository(AppDbContext db)
    {
        Db = db;
        Set = db.Set<T>();
    }

    public virtual Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Set.FindAsync(new object[] { id }, ct).AsTask();

    public virtual async Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default)
        => await Set.AsNoTracking().ToListAsync(ct);

    public virtual async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await Set.AsNoTracking().Where(predicate).ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        => await Set.AddAsync(entity, ct);

    public virtual void Update(T entity) => Set.Update(entity);

    public virtual void Remove(T entity) => Set.Remove(entity);

    public virtual Task<int> SaveChangesAsync(CancellationToken ct = default)
        => Db.SaveChangesAsync(ct);
}
