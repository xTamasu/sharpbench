// EfRepository — generic EF Core implementation of IRepository<T>.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public EfRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, ct);
    }

    public async Task<IQueryable<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await Task.FromResult(_dbSet.AsQueryable());
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, ct) != null;
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, ct);
    }
}
