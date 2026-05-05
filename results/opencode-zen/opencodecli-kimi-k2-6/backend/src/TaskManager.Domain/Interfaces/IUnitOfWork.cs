namespace TaskManager.Domain.Interfaces;

/// <summary>
/// Unit of work pattern for coordinating repository operations and saving changes.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
