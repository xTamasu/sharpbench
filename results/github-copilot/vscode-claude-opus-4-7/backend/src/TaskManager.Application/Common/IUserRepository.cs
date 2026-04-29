// IUserRepository.cs
// Entity-specific repository extension for User (lookup by email).
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Common;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
}
