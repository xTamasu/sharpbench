// UserRepository.cs
// EF Core implementation of IUserRepository.
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(u => u.Email == email, ct);
}
