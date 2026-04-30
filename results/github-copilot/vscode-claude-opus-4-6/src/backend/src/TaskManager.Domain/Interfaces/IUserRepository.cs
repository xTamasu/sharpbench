// User repository interface: extends generic repository with user-specific queries.
using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}
