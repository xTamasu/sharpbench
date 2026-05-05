// JWT token service interface — generates and validates JWT tokens.

using System;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
